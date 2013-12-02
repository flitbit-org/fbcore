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

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   A parallel reactor is used to efficiently trigger actions in parallel in
  ///   response to items being pushed to the reactor.
  /// </summary>
  /// <typeparam name="TItem">item type TItem</typeparam>
  public class Reactor<TItem> : ReactorBase
  {
    readonly Object _lock = new Object();
    readonly ReactorOptions _options;
    readonly ConcurrentQueue<TItem> _queue = new ConcurrentQueue<TItem>();
    readonly Action<Reactor<TItem>, TItem> _reactor;
    readonly Thread _backgroundThread;

    int _backgroundWorkers;
    int _backgroundWorkersActive;
    int _backgroundWorkersScheduled;
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
      Interlocked.Increment(ref _backgroundWorkers);
    }

    /// <summary>
    ///   Indicates whether the reactor is active.
    /// </summary>
    public bool IsActive { get { return _backgroundThread.IsAlive; } }

    /// <summary>
    ///   Indicates whethe the reactor is stopping.
    /// </summary>
    public bool IsCanceled { get { return Util.VolatileRead(ref _canceled); } }

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
        lock (_lock)
        {
          return _backgroundWorkersActive == 0
                 && _backgroundWorkersScheduled == 0;
        }
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

      if (_queue.Count > _options.MaxParallelDepth)
      {
        if (!IsForegroundThreadBorrowed)
        {
          try
          {
            IsForegroundThreadBorrowed = true;
            Foreground_Reactor(item, _options.DispatchesPerBorrowedThread);
          }
          finally
          {
            IsForegroundThreadBorrowed = false;
          }
        }
      }
      else
      {
        _queue.Enqueue(item);
      }

      CheckBackgroundReactorState();
      return this;
    }

    void ReactorControllerLogic()
    {
      var itemsHandled = 0;
      try
      {
        if (LogSink.IsLogging(SourceLevels.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Entering reactor controller logic: {0} of {1}", active, workers)
            );
        }
        while (!IsCanceled)
        {
          var pre = _queue.Count;
          if (pre > 0)
          {
            itemsHandled += PerformReactorLogic();
            var post = _queue.Count;
            if (post > (pre + 1))
            { // filling faster than draining, create more background tasks in the threadpool...
              Interlocked.Increment(ref _backgroundWorkers);
              Task.Factory.StartNew(Background_Reactor);
            }
            else if (post == 0)
            {
              
            }
          }
        }
      }
      finally
      {
        var remaining = Interlocked.Decrement(ref _backgroundWorkersActive);
        try
        {
          if (LogSink.IsLogging(SourceLevels.Verbose))
          {
            LogSink.Verbose(
              String.Format(
                "Exiting reactor controller logic; handled {0} items, {1} remaining workers", itemsHandled,
                Thread.VolatileRead(ref _backgroundWorkers) - 1)
              );
          }
        }
        finally
        {
          Interlocked.Decrement(ref _backgroundWorkers);
        }
      }
    }

    int PerformReactorLogic()
    {
      var count = 0;
      TItem item;
      if (!IsCanceled
          && _queue.TryDequeue(out item))
      {
        Interlocked.Increment(ref _backgroundWorkersActive);
        try
        {
          count++;
          _reactor(this, item);
        }
        catch (Exception e)
        {
          if (LogSink.IsLogging(SourceLevels.Error))
          {
            LogSink.Verbose(
              String.Format("Reactor threw an uncaught exception: {0}", e.FormatForLogging()));
          }
          if (OnUncaughtException(e))
          {
            throw;
          }
        }
        finally
        {
          Interlocked.Decrement(ref _backgroundWorkersActive);
        }
      }
      return count;
    }

    void Background_Reactor()
    {
      var itemsHandled = 0;
      try
      {
        int workers = Thread.VolatileRead(ref this._backgroundWorkers);
        int active = Interlocked.Increment(ref this._backgroundWorkersActive);
        if (LogSink.IsLogging(SourceLevels.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Entering background reactor logic: {0} of {1}", active, workers)
            );
        }
        // Continue until signaled or no more items in queue...
        while (!IsCanceled)
        {
          var count = PerformReactorLogic();
          itemsHandled += count;
          if (count == 0) break;
        }
      }
      finally
      {
        try
        {
          if (LogSink.IsLogging(SourceLevels.Verbose))
          {
            LogSink.Verbose(
              String.Format(
                "Exiting background reactor; handled {0} items, {1} remaining workers", itemsHandled,
                Thread.VolatileRead(ref _backgroundWorkers) - 1)
              );
          }
        }
        finally
        {
          Interlocked.Decrement(ref _backgroundWorkers);
        }
      }
    }

    void Foreground_Reactor(TItem item, int dispatchesPerSequential)
    {
      var itemsHandled = 0;
      try
      {
        int workers = Thread.VolatileRead(ref this._backgroundWorkers);
        int active = Interlocked.Increment(ref this._backgroundWorkersActive);
        if (LogSink.IsLogging(SourceLevels.Verbose))
        {
          LogSink.Verbose(
            String.Format(
              "Entering borrowed foreground thread: {0} of {1}", active, workers)
            );
        }
        while (!IsCanceled && itemsHandled < dispatchesPerSequential)
        {
          var count = PerformReactorLogic();
          itemsHandled += count;
          if (count == 0) break;
        }
      }
      finally
      {
        try
        {
          if (LogSink.IsLogging(SourceLevels.Verbose))
          {
            LogSink.Verbose(
              String.Format(
                "Exiting borrowed foreground thread; handled {0} items, {1} remaining workers", itemsHandled,
                Thread.VolatileRead(ref _backgroundWorkers) - 1)
              );
          }
        }
        finally
        {
          Interlocked.Decrement(ref _backgroundWorkers);
        }
      }
    }

  }
}