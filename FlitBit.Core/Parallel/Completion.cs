#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Default waitable implementation.
  /// </summary>
  public class Completion : Disposable
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly ContinuationSet _continuations;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    AsyncResult _asyncResult;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ContextFlow _context;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Future<bool> _future = new Future<bool>();

    /// <summary>
    ///   Constructs a new instance.
    /// </summary>
    public Completion(Object target)
      : this(target, false) { }

    /// <summary>
    ///   Constructs a new instance.
    /// </summary>
    /// <param name="target">action's target</param>
    /// <param name="completed">
    ///   Indicates whether the wait has already
    ///   completed.
    /// </param>
    public Completion(Object target, bool completed)
    {
      Target = target;
      if (completed)
      {
        _future.MarkCompleted(true);
      }
      _context = ContextFlow.ForkAmbient();
      _continuations = new ContinuationSet(ContextFlow.ForkAmbient());
    }

    /// <summary>
    ///   Gets the exception that caused the fault.
    /// </summary>
    public Exception Exception { get { return _future.Exception; } }

    /// <summary>
    ///   Indicates whether the wait has completed.
    /// </summary>
    public bool IsCompleted { get { return _future.IsCompleted; } }

    /// <summary>
    ///   Determines if the completion resulted in an error.
    /// </summary>
    public bool IsFaulted { get { return _future.IsFaulted; } }

    /// <summary>
    ///   The completion's target object if given when the completion was created.
    /// </summary>
    public Object Target { get; private set; }

    /// <summary>
    ///   Gets a wait handle for the completion.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public WaitHandle WaitHandle
    {
      get
      {
        Contract.Requires<ObjectDisposedException>(!IsDisposed);

        return _future.WaitHandle;
      }
    }

    /// <summary>
    ///   Schedules an action to execute when this completion is done.
    /// </summary>
    /// <param name="continuation">a continuation to run upon completion.</param>
    public void Continue(Continuation continuation)
    {
      Contract.Requires<ArgumentNullException>(continuation != null);
      _continuations.Continue(continuation);
    }

    /// <summary>
    ///   Schedules an action to execute when another completion succeeds.
    /// </summary>
    /// <param name="continuation">an action to run when the completion succeeds</param>
    /// <returns>a completion for the success action</returns>
    public Completion ContinueWithCompletion(Continuation continuation)
    {
      Contract.Requires<ArgumentNullException>(continuation != null);
      Contract.Ensures(Contract.Result<Completion>() != null);
      return _continuations.ContinueWithCompletion(continuation);
    }

    /// <summary>
    ///   Schedules a function to execute when another completion succeeds.
    /// </summary>
    /// <typeparam name="TResult">result type R</typeparam>
    /// <param name="continuation">a function to run when the completion succeeds</param>
    /// <returns>a completion for the success function</returns>
    public Completion<TResult> ContinueWithCompletion<TResult>(ContinuationFunc<TResult> continuation)
    {
      Contract.Requires<ArgumentNullException>(continuation != null);
      Contract.Ensures(Contract.Result<Completion<TResult>>() != null);
      return _continuations.ContinueWithCompletion(continuation);
    }

    /// <summary>
    ///   Makes an AsyncCallback delegate that produces the completion.
    /// </summary>
    /// <typeparam name="THandback">handback type H</typeparam>
    /// <param name="handback">the handback</param>
    /// <param name="handler">a handler that produces the completion</param>
    /// <returns>An AsyncCallback.</returns>
    public AsyncCallback MakeAsyncCallback<THandback>(THandback handback, Action<IAsyncResult, THandback> handler)
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);

      return ar =>
      {
        try
        {
          handler(ar, handback);
          MarkCompleted();
        }
        catch (Exception e)
        {
          MarkFaulted(e);
        }
      };
    }

    /// <summary>
    ///   Marks the completion.
    /// </summary>
    public void MarkCompleted()
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);
      _future.MarkCompleted(true);
      _continuations.NotifyCompletion(null);
    }

    /// <summary>
    ///   Marks the completion.
    /// </summary>
    /// <param name="fault"></param>
    public void MarkFaulted(Exception fault)
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);
      _future.MarkFaulted(fault);
      _continuations.NotifyCompletion(fault);
    }

    /// <summary>
    ///   Gets an async result for .NET framework synchronization.
    /// </summary>
    /// <returns></returns>
    public AsyncResult ToAsyncResult()
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);

      return ToAsyncResult(null, null, null);
    }

    /// <summary>
    ///   Gets an async result for .NET framework synchronization.
    /// </summary>
    /// <param name="asyncCallback"></param>
    /// <param name="asyncHandback"></param>
    /// <returns></returns>
    public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback)
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);

      return ToAsyncResult(asyncCallback, asyncHandback, null);
    }

    /// <summary>
    ///   Gets an async result for .NET framework synchronization.
    /// </summary>
    /// <param name="asyncCallback"></param>
    /// <param name="asyncHandback"></param>
    /// <param name="asyncState"></param>
    /// <returns></returns>
    public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback, Object asyncState)
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);
      if (_asyncResult == null)
      {
        lock (_future.SyncObject)
        {
          if (_asyncResult == null)
          {
            var ar = new AsyncResult(asyncCallback, asyncHandback, asyncState);
            // ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
            Util.VolatileWrite(out _asyncResult, ar);
            // ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
            this.Continue(e =>
            {
              if (e != null)
              {
                ar.MarkException(e, false);
              }
              else
              {
                ar.MarkCompleted(false);
              }
            });
          }
        }
      }
      return _asyncResult;
    }

    /// <summary>
    ///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
    /// </summary>
    /// <param name="timeout">A timespan representing the timeout period.</param>
    /// <returns>The future's value.</returns>
    public bool Wait(TimeSpan timeout)
    {
      Contract.Requires<ObjectDisposedException>(!IsDisposed);

      return _future.Wait(timeout);
    }

    /// <summary>
    ///   Performs dispose on the completion.
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override bool PerformDispose(bool disposing)
    {
      if (disposing)
      {
        Util.Dispose(ref _future);
        Util.Dispose(ref _context);
      }
      return true;
    }
  }
}