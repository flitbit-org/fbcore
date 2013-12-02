#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Utility for notify
  /// </summary>
  [Obsolete("Actually, just not completed yet. Do not use.")]
  public sealed class Notification
  {
    static readonly Lazy<Notification> Singleton = new Lazy<Notification>(() => new Notification(),
      LazyThreadSafetyMode.ExecutionAndPublication);

    static int __recordId;

    readonly ConcurrentQueue<NotifyRecord> _incomming = new ConcurrentQueue<NotifyRecord>();
    readonly Reactor<NotifyRecord> _invoker = new Reactor<NotifyRecord>((self, rec) => rec.Handler());
    readonly Lazy<Notifier> _notifier;

    Notification()
    {
      _notifier = new Lazy<Notifier>(() => new Notifier(_incomming, _invoker),
        LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    ///   Schedules a continuation action for after an async result
    ///   completes.
    /// </summary>
    /// <param name="async">the async result</param>
    /// <param name="after">the continuation</param>
    public void ContinueWith(IAsyncResult async, Action after)
    {
      Contract.Requires<ArgumentNullException>(async != null);
      Contract.Requires<ArgumentNullException>(after != null);

      var ambient = ContextFlow.ForkAmbient();
      var record = new NotifyRecord
      {
        ID = Interlocked.Increment(ref __recordId),
        Async = async,
        Handler = () =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              after();
            }
            catch (Exception e)
            {
              ContextFlow.NotifyUncaughtException(after.Target, e);
            }
          }
        }
      };
      _incomming.Enqueue(record);
      _notifier.Value.Wake();
    }

    /// <summary>
    ///   Accesses the (Lazy) notification instance.
    /// </summary>
    public static Notification Instance { get { return Singleton.Value; } }

    class Notifier : Disposable
    {
      const int CRecordsPerWaitPeriod = 63;
      const int CStatusNone = 0;
      const int CStatusOk = 1;
      const int CStatusRepartition = 3;
      const int CStatusStopped = 6;
      const int CStatusStopping = 5;
      const int CStatusWaiting = 2;

      static int __threadId;

      readonly ManualResetEvent _alert;
      readonly int _id;
      readonly Reactor<NotifyRecord> _notifier;
      readonly ConcurrentQueue<NotifyRecord> _queue;
      readonly AutoResetEvent _signal;
      readonly Thread _waiter;

      int _status = CStatusNone;

      internal Notifier(ConcurrentQueue<NotifyRecord> queue, Reactor<NotifyRecord> notifier)
      {
        _id = Interlocked.Increment(ref __threadId);
        _queue = queue;
        _notifier = notifier;
        _alert = new ManualResetEvent(false);
        _signal = new AutoResetEvent(false);
        _waiter = new Thread(PerformLeaderLogic);
        _waiter.Start();
      }

      internal int ID { get { return _id; } }

      protected override bool PerformDispose(bool disposing)
      {
        if (disposing)
        {
          Thread.VolatileWrite(ref _status, CStatusStopping);
          _alert.Set();
          _signal.Set();
          Thread.Yield();
          _signal.Dispose();
          _alert.Dispose();
        }
        return true;
      }

      internal void Wake()
      {
        var res = Thread.VolatileRead(ref _status);
        while (res < CStatusStopping)
        {
          if (res == CStatusRepartition)
          {
            break; // if currently repartitioning then no need to wake.
          }
          if (res == CStatusWaiting
              && Interlocked.CompareExchange(ref _status, CStatusRepartition, CStatusWaiting) == CStatusWaiting)
          {
            _signal.Set();
            break; // was waiting and we set status to repartitioning.
          }
          // lost race condition, loop to ensure it is still ok...
          res = Thread.VolatileRead(ref _status);
        }
      }

      void PerformFollowerLogic(object state)
      {
        const int wake = 0;
        const int offset = -1;
        var status = (NotifierState)state;
        status.MarkOk(CStatusNone, null);
        try
        {
          int res;
          while ((res = status.Status) < CStatusStopping)
          {
            var items = status.Records;
            switch (res)
            {
              case CStatusOk:
              case CStatusWaiting:
                if (items == null
                    || !items.Any())
                {
                  status.Alert.WaitOne();
                }
                else
                {
                  var waiting = new WaitHandle[]
                  {
                    status.Alert
                  }.Concat(
                    from r in items
                    where r.Async.AsyncWaitHandle != null
                    select r.Async.AsyncWaitHandle
                    )
                   .ToArray();
                  if (res == status.MarkWaiting(res))
                  {
                    var signaled = WaitHandle.WaitAny(waiting);
                    if (signaled > wake)
                    {
                      var it = items[signaled + offset];
                      items.RemoveAt(signaled + offset);
                      _notifier.Push(it);
                    }
                  }
                }
                break;
              case CStatusRepartition:
                if (items != null)
                {
                  var q = status.SharedQueue;
                  foreach (var it in items)
                  {
                    q.Enqueue(it);
                  }
                }
                status.MarkOk(CStatusRepartition, null);
                break;
            }
          }
        }
        catch (ThreadAbortException)
        {}
        catch (Exception e)
        {
          ContextFlow.NotifyUncaughtException(this, e);
        }
        Thread.VolatileWrite(ref _status, CStatusStopped);
      }

      void PerformLeaderLogic()
      {
        var listeners = new List<NotifierState>();
        try
        {
          int res;
          while ((res = Thread.VolatileRead(ref _status)) < CStatusStopping)
          {
            if (res == CStatusRepartition)
            {
              if (_queue.Count > 0)
              {
                var candidates = new List<NotifierState>();
                foreach (var c in listeners)
                {
                  if (c.MarkRepartition())
                  {
                    candidates.Add(c);
                  }
                  else
                  {
                    c.Stop();
                  }
                }
                _alert.Set();
                foreach (var c in candidates)
                {
                  c.WaitFor(CStatusOk);
                }
                _alert.Reset();
                NotifyRecord rec;
                var records = new List<NotifyRecord>();
                while (_queue.TryDequeue(out rec))
                {
                  if (rec.Async.IsCompleted)
                  {
                    _notifier.Push(rec);
                  }
                  else
                  {
                    records.Add(rec);
                  }
                }
                var arr = records.ToArray();
                int i = 0, j = 0;
                while (j < records.Count)
                {
                  var ub = Math.Min(CRecordsPerWaitPeriod, records.Count - j);
                  var slice = new NotifyRecord[ub];
                  Array.Copy(arr, j, slice, 0, ub);
                  j += ub;
                  var items = new List<NotifyRecord>(slice);

                  NotifierState sub;
                  if (i == candidates.Count)
                  {
                    candidates.Add(sub = new NotifierState
                    {
                      ID = Interlocked.Increment(ref __threadId),
                      Alert = _alert,
                      SharedQueue = _queue,
                      Notifier = _notifier
                    });
                    new Thread(PerformFollowerLogic).Start(sub);
                    sub.MarkOk(CStatusNone, items);
                  }
                  else
                  {
                    sub = candidates[i];
                    sub.MarkOk(CStatusRepartition, items);
                  }

                  i++;
                }
                _alert.Set();
                _alert.Reset();
                listeners = new List<NotifierState>(candidates);
              }
              Interlocked.CompareExchange(ref _status, CStatusOk, CStatusRepartition);
            }
            else
            {
              if (Interlocked.CompareExchange(ref _status, CStatusWaiting, res) == res)
              {
                _signal.WaitOne();
              }
            }
          }
        }
        catch (ThreadAbortException)
        {}
        catch (Exception e)
        {
          ContextFlow.NotifyUncaughtException(this, e);
        }
        Thread.VolatileWrite(ref _status, CStatusStopped);
      }

      class NotifierState
      {
        List<NotifyRecord> _records;
        int _status = CStatusNone;

        public ManualResetEvent Alert { get; set; }
        public int ID { get; set; }
        public Reactor<NotifyRecord> Notifier { get; set; }

        public List<NotifyRecord> Records { get { return Util.VolatileRead(ref _records); } }

        public ConcurrentQueue<NotifyRecord> SharedQueue { get; set; }

        public int Status { get { return Thread.VolatileRead(ref _status); } }

        public int MarkOk(int expect, List<NotifyRecord> records)
        {
          var res = Interlocked.CompareExchange(ref _status, CStatusOk, expect);
          if (res == expect
              && records != null)
          {
            Util.VolatileWrite(out _records, records);
          }
          return res;
        }

        public bool MarkRepartition()
        {
          var res = Thread.VolatileRead(ref _status);
          while (res < CStatusStopping)
          {
            if (Interlocked.CompareExchange(ref _status, CStatusRepartition, res) == res)
            {
              return true;
            }
            // lost race condition, loop to ensure it is still ok...
            res = Thread.VolatileRead(ref _status);
          }
          return false;
        }

        public void MarkStopped() { Thread.VolatileWrite(ref _status, CStatusStopped); }

        public int MarkWaiting(int expect) { return Interlocked.CompareExchange(ref _status, CStatusWaiting, expect); }

        public void Stop()
        {
          var res = Thread.VolatileRead(ref _status);
          if (res < CStatusStopping)
          {
            Thread.VolatileWrite(ref _status, CStatusStopping);
          }
        }

        internal void WaitFor(int state)
        {
          while (Status != state)
          {
            Thread.Yield();
          }
        }
      }
    }

    internal struct NotifyRecord
    {
      public IAsyncResult Async;
      public WaitHandle Handle;
      public Action Handler;
      public int ID;
    }
  }
}