#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using FlitBit.Core.Log;
using ThreadState = System.Threading.ThreadState;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   A parallel reactor is used to efficiently trigger actions in parallel in
  ///   response to items being pushed to the reactor.
  /// </summary>
  /// <typeparam name="TItem">item type TItem</typeparam>
  public class Reactor<TItem> : ReactorBase
  {
    readonly ReactorOptions _options;
    readonly ConcurrentQueue<Tuple<TItem, ContextFlow>> _queue = new ConcurrentQueue<Tuple<TItem, ContextFlow>>();
    readonly Action<Reactor<TItem>, TItem> _reactor;
    readonly Thread _backgroundThread;

    int _backgroundWorkersScheduled;
    int _foregroundWorkers;
    int _backgroundWorkers;
    bool _canceled;

    /// <summary>
    ///   Creates a new instance with the default options.
    /// </summary>
    /// <param name="reactor">the reactor's action</param>
    public Reactor(Action<Reactor<TItem>, TItem> reactor)
      : this(reactor, DefaultOptions)
    {}

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="reactor">the reactor's action</param>
    /// <param name="options">options</param>
    public Reactor(Action<Reactor<TItem>, TItem> reactor, ReactorOptions options)
    {
      Contract.Requires<ArgumentNullException>(reactor != null);

      _reactor = reactor;
      _options = options ?? DefaultOptions;
      _backgroundThread = new Thread(ReactorControllerLogic);
      _backgroundThread.IsBackground = true;
      _backgroundThread.Start();
      Interlocked.Increment(ref _backgroundWorkersScheduled);
    }

    /// <summary>
    ///   Indicates whether the reactor is active.
    /// </summary>
    public bool IsActive { get { return _backgroundThread.IsAlive; } }

    /// <summary>
    ///   Indicates whethe the reactor is stopping.
    /// </summary>
    public bool IsCanceled {
      get
      {
        return Util.VolatileRead(ref _canceled)
               || Util.VolatileRead(ref AppDomainUnloaded);
      } 
    }

    /// <summary>
    ///   Determines if the reactor is empty. Empty means there are no items
    ///   that have not already been reacted too.
    /// </summary>
    public bool IsEmpty { get { return _queue.IsEmpty; } }

    /// <summary>
    ///   Indicates whether the reactor is idle.
    /// </summary>
    public bool IsIdle
    {
      get
      {
        return Thread.VolatileRead(ref _backgroundWorkers) == 1
               && _queue.Count == 0;
      }
    }

    /// <summary>
    ///   Indicates whether the reactor is stopped.
    /// </summary>
    public bool IsStopped { get { return IsCanceled && IsIdle; } }

    /// <summary>
    ///   Gets the reactor's options.
    /// </summary>
    public ReactorOptions Options { get { return _options; } }

    /// <summary>
    ///   Stops a reactor. Once stopped a reactor cannot be restarted.
    /// </summary>
    /// <returns>the reactor (for chaining)</returns>
    public Reactor<TItem> Cancel()
    {
      Util.VolatileWrite(out _canceled, true);
      return this;
    }

    /// <summary>
    ///   Pushes a new item to the reactor.
    /// </summary>
    /// <param name="item">an item</param>
    /// <returns>the reactor (for chaining)</returns>
    public Reactor<TItem> Push(TItem item)
    {
      Contract.Requires<InvalidOperationException>(!IsCanceled);

      var itemAndContext = (_options.CaptureCallerContext)
                                                   ? Tuple.Create(item, ContextFlow.ForkAmbient())
                                                   : Tuple.Create(item, (ContextFlow)null);

     
      if (_queue.Count > _options.MaxParallelDepth
          && !IsForegroundThreadBorrowed)  // prevents unexpected cycle through the reactor's action.
      {
        try
        {
          IsForegroundThreadBorrowed = true;
          Foreground_Reactor(itemAndContext, _options.DispatchesPerBorrowedThread);
        }
        finally
        {
          IsForegroundThreadBorrowed = false;
        }
      }
      else
      {
        _queue.Enqueue(itemAndContext);
      }

      return this;
    }

    void ReactorControllerLogic()
    {
      var itemsHandled = 0;
      Interlocked.Increment(ref _backgroundWorkers);
      Interlocked.Decrement(ref _backgroundWorkersScheduled);
      try
      {
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose("Entering reactor controller logic.");
        }
        while (!IsCanceled)
        {
          var pre = _queue.Count;
          var cycle = 0;
          if (pre > 0)
          {
            cycle += PerformReactorLogic(null, 1);
            itemsHandled += cycle;
            var post = _queue.Count;
            if (post > pre
                && post > (cycle + 2)
                && Thread.VolatileRead(ref _backgroundWorkers) < _options.MaxDegreeOfParallelism)
            { // filling faster than draining, create more background tasks in the threadpool...
              Interlocked.Increment(ref _backgroundWorkersScheduled);
              Task.Factory.StartNew(Background_Reactor);
            }
            else if (post == 0)
            {
              PerformIdlingLogic();
            }
          }
        }
      }
      finally
      {
        Interlocked.Decrement(ref _backgroundWorkers);
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Leaving reactor controller logic; handled {0} total items, {1} remaining background workers and {2} borrowed foreground threads.", itemsHandled,
              Thread.VolatileRead(ref _backgroundWorkers),
              Thread.VolatileRead(ref _foregroundWorkers)
              )
            );
        }
      }
    }

    /// <summary>
    /// Perform any idling logic.
    /// </summary>
    protected virtual void PerformIdlingLogic()
    {
      Thread.Yield();
    }

    int PerformReactorLogic(Tuple<TItem, ContextFlow> itemAndContext, int limit)
    {
      var count = 0;
      var withContext = _options.CaptureCallerContext;
      if (itemAndContext != null)
      {
        if (withContext) HandleItemWithContext(itemAndContext.Item1, itemAndContext.Item2);
        else HandleItem(itemAndContext.Item1);
        count++;
      }
      Tuple<TItem, ContextFlow> item;
      while(!IsCanceled && count < limit && _queue.TryDequeue(out item))
      {
        if (withContext) HandleItemWithContext(item.Item1, item.Item2);
        else HandleItem(item.Item1);
        count++;
      }
      return count;
    }

    private void HandleItem(TItem item)
    {
      try
      {
        _reactor(this, item);
      }
      catch (Exception e)
      {
        if (LogSink.IsLogging(TraceEventType.Error))
        {
          LogSink.Verbose(
            String.Format("Reactor threw an uncaught exception: {0}", e.FormatForLogging()));
        }
        if (OnUncaughtException(e))
        {
          throw;
        }
      }
    }

    private void HandleItemWithContext(TItem item, ContextFlow contextFlow)
    {
      try
      {
        using (ContextFlow.EnsureAmbient(contextFlow))
        {
          try
          {
            _reactor(this, item);
          }
          catch (Exception e)
          {
            ContextFlow.NotifyUncaughtException(_reactor.Target, e);
            throw;
          }
        }
      }
      catch (Exception e)
      {
        if (LogSink.IsLogging(TraceEventType.Error))
        {
          LogSink.Verbose(
            String.Format("Reactor threw an uncaught exception: {0}", e.FormatForLogging()));
        }
        if (OnUncaughtException(e))
        {
          throw;
        }
      }
    }

    void Background_Reactor()
    {
      Interlocked.Increment(ref _backgroundWorkers);
      Interlocked.Decrement(ref _backgroundWorkersScheduled);
      var itemsHandled = 0;
      try
      {
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Entering background reactor logic: {0} background workers and {1} borrowed foreground threads.",
              Thread.VolatileRead(ref _backgroundWorkers),
              Thread.VolatileRead(ref _foregroundWorkers))
            );
        }
        // Continue until signaled or no more items in queue...
        while (!IsCanceled)
        {
          var count = PerformReactorLogic(null, _options.YieldFrequency);
          itemsHandled += count;
          if (count == 0) break;
        }
      }
      finally
      {
        Interlocked.Decrement(ref _backgroundWorkers);
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Leaving background reactor logic; handled {0} items, {1} remaining background workers and {2} borrowed foreground threads.",
              itemsHandled,
              Thread.VolatileRead(ref _backgroundWorkers),
              Thread.VolatileRead(ref _foregroundWorkers))
            );
        }
      }
    }

    void Foreground_Reactor(Tuple<TItem, ContextFlow> itemAndContext, int dispatchesPerSequential)
    {
      Interlocked.Increment(ref _foregroundWorkers);
      var itemsHandled = 0;
      try
      {
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Entering borrowed foreground reactor logic: {0} background workers and {1} borrowed foreground threads.",
              Thread.VolatileRead(ref _backgroundWorkers),
              Thread.VolatileRead(ref _foregroundWorkers))
            );
        }
        itemsHandled = PerformReactorLogic(itemAndContext, dispatchesPerSequential);
      }
      finally
      {
        Interlocked.Decrement(ref _foregroundWorkers);
        if (LogSink.IsLogging(TraceEventType.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Leaving borrowed foreground reactor logic; handled {0} items, {1} remaining background workers and {2} borrowed foreground threads.",
              itemsHandled,
              Thread.VolatileRead(ref _backgroundWorkers),
              Thread.VolatileRead(ref _foregroundWorkers))
            );
        }
      }
    }

  }
}