#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Parallelism utilities.
  /// </summary>
  public static class Go
  {
    static EventHandler<UncaughtExceptionArgs> __onUncaughtException;

    /// <summary>
    ///   Event fired when uncaught exceptions are raised by parallel operations.
    /// </summary>
    public static event EventHandler<UncaughtExceptionArgs> OnUncaughtException
    {
      add { __onUncaughtException += value; }
// ReSharper disable DelegateSubtraction
      remove { __onUncaughtException -= value; }
// ReSharper restore DelegateSubtraction
    }

    /// <summary>
    ///   Notifies event handlers that an exception has occurred in a paralle operation.
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="e">the exception that was raised.</param>
    public static void NotifyUncaughtException(object sender, Exception e)
    {
      if (__onUncaughtException != null)
      {
        __onUncaughtException(sender, new UncaughtExceptionArgs(e));
      }
    }

    /// <summary>
    ///   Performs an action in parallel.
    /// </summary>
    /// <param name="action">an action</param>
    public static void Parallel(Action action)
    {
      Contract.Requires<ArgumentNullException>(action != null);

      var ambient = ContextFlow.ForkAmbient();
      ThreadPool.QueueUserWorkItem(
        ignored =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              action();
            }
            catch (Exception e)
            {
              NotifyUncaughtException(action.Target, e);
            }
          }
        });
    }

    /// <summary>
    ///   Performs an action in parallel.
    /// </summary>
    /// <param name="action">an action</param>
    /// <param name="continuation">a continuation called upon completion.</param>
    public static void Parallel(Action action, Continuation continuation)
    {
      Contract.Requires<ArgumentNullException>(action != null);
      Contract.Requires<ArgumentNullException>(continuation != null);

      var ambient = ContextFlow.ForkAmbient();
      ThreadPool.QueueUserWorkItem(
        ignored =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              try
              {
                action();
              }
              catch (Exception e)
              {
                continuation(e);
                return;
              }
              continuation(null);
            }
            catch (Exception e)
            {
              NotifyUncaughtException(continuation.Target, e);
            }
          }
        });
    }

    /// <summary>
    ///   Performs the function in parallel.
    /// </summary>
    /// <typeparam name="TResult">result type R</typeparam>
    /// <param name="fun">an action</param>
    /// <param name="continuation">a continuation called upon completion.</param>
    public static void Parallel<TResult>(Func<TResult> fun, Continuation<TResult> continuation)
    {
      Contract.Requires<ArgumentNullException>(fun != null);
      Contract.Requires<ArgumentNullException>(continuation != null);

      var ambient = ContextFlow.ForkAmbient();
      ThreadPool.QueueUserWorkItem(
        ignored =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              continuation(null, fun());
            }
            catch (Exception e)
            {
              try
              {
                continuation(e, default(TResult));
              }
              catch (Exception ee)
              {
                NotifyUncaughtException(continuation.Target, ee);
              }
            }
          }
        });
    }

    /// <summary>
    ///   Performs an action in parallel.
    /// </summary>
    /// <param name="action">an action</param>
    /// <returns>a completion</returns>
    public static Completion ParallelWithCompletion(Action action)
    {
      Contract.Requires<ArgumentNullException>(action != null);
      Contract.Ensures(Contract.Result<Completion>() != null);

      var waitable = new Completion(action.Target);
      var ambient = ContextFlow.ForkAmbient();
      ThreadPool.QueueUserWorkItem(
        ignored =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              action();
              waitable.MarkCompleted();
            }
            catch (Exception e)
            {
              waitable.MarkFaulted(e);
            }
          }
        });
      return waitable;
    }

    /// <summary>
    ///   Performs the function in parallel.
    /// </summary>
    /// <typeparam name="TResult">result type R</typeparam>
    /// <param name="fun">an action</param>
    /// <returns>a completion where the results will be returned upon completion</returns>
    public static Completion<TResult> ParallelWithCompletion<TResult>(Func<TResult> fun)
    {
      Contract.Requires<ArgumentNullException>(fun != null);
      Contract.Ensures(Contract.Result<Completion<TResult>>() != null);

      var waitable = new Completion<TResult>(fun.Target);
      var ambient = ContextFlow.ForkAmbient();
      ThreadPool.QueueUserWorkItem(
        ignored =>
        {
          using (ContextFlow.EnsureAmbient(ambient))
          {
            try
            {
              waitable.MarkCompleted(fun());
            }
            catch (Exception e)
            {
              waitable.MarkFaulted(e);
            }
          }
        });
      return waitable;
    }
  }
}