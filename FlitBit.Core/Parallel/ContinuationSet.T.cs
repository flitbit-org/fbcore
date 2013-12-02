using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   A set of continuations, each signaled once.
  /// </summary>
  public class ContinuationSet<T>
  {
    readonly ContextFlow _context;

    readonly ConcurrentQueue<ContinuationNotifier> _continuations = new ConcurrentQueue<ContinuationNotifier>();
    int _completed;
    Exception _fault;
    T _res;

    internal ContinuationSet(ContextFlow context) { _context = context; }

    internal void Continue(Continuation<T> continuation)
    {
      try
      {
        _continuations.Enqueue(new ContinuationNotifier(continuation));
      }
      finally
      {
        if (Thread.VolatileRead(ref _completed) > 0)
        {
          ContextFlow.Parallel(background_Notifier);
        }
      }
    }

    internal Completion ContinueWithCompletion(Continuation<T> continuation)
    {
      var waitable = new Completion(continuation.Target);
      try
      {
        _continuations.Enqueue(new ContinuationNotifierWithCompletion(continuation, waitable));
      }
      finally
      {
        if (Thread.VolatileRead(ref _completed) > 0)
        {
          ContextFlow.Parallel(background_Notifier);
        }
      }
      return waitable;
    }

    internal Completion<TResult> ContinueWithCompletion<TResult>(ContinuationFunc<T, TResult> continuation)
    {
      var waitable = new Completion<TResult>(continuation.Target);
      try
      {
        _continuations.Enqueue(new ContinuationNotifierWithCompletion<TResult>(continuation, waitable));
      }
      finally
      {
        if (Thread.VolatileRead(ref _completed) > 0)
        {
          ContextFlow.Parallel(background_Notifier);
        }
      }
      return waitable;
    }

    internal void NotifyCompletion(Exception e, T res)
    {
      Util.VolatileWrite(out _fault, e);
      Util.VolatileWrite(out _res, res);
      Interlocked.Increment(ref _completed);
      ContextFlow.Parallel(background_Notifier);
    }

    void background_Notifier()
    {
      var e = Util.VolatileRead(ref _fault);
      var res = Util.VolatileRead(ref _res);
      ContinuationNotifier notifier;
      while (_continuations.TryDequeue(out notifier))
      {
        notifier.ContinueWith(_context, e, res);
      }
    }

    class ContinuationNotifier
    {
      readonly Delegate _delg;
      int _continued;

      public ContinuationNotifier(Delegate delg) { this._delg = delg; }

      public void ContinueWith(ContextFlow context, Exception e, T res)
      {
        if (Interlocked.CompareExchange(ref _continued, 1, 0) == 0)
        {
          ThreadPool.QueueUserWorkItem(unused =>
          {
            if (context != null)
            {
              ContextFlow.Ambient.Push(context);
            }
            try
            {
              PerformContinuation(_delg, e, res);
            }
            finally
            {
              if (context != null)
              {
                ContextFlow.Ambient.TryPop(context);
              }
            }
          });
        }
      }

      protected virtual void PerformContinuation(Delegate delg, Exception e, T res)
      {
        try
        {
          ((Continuation<T>)delg)(e, res);
        }
        catch (Exception ee)
        {
          ContextFlow.NotifyUncaughtException(delg.Target, ee);
        }
      }
    }

    class ContinuationNotifierWithCompletion : ContinuationNotifier
    {
      readonly Completion _comp;

      public ContinuationNotifierWithCompletion(Continuation<T> delg, Completion comp)
        : base(delg) { this._comp = comp; }

      protected override void PerformContinuation(Delegate delg, Exception e, T res)
      {
        try
        {
          ((Continuation<T>)delg)(e, res);
          try
          {
            _comp.MarkCompleted();
          }
          catch (Exception ee)
          {
            ContextFlow.NotifyUncaughtException(delg.Target, ee);
          }
        }
        catch (Exception ee)
        {
          try
          {
            _comp.MarkFaulted(ee);
          }
          catch (Exception eee)
          {
            ContextFlow.NotifyUncaughtException(delg.Target, eee);
          }
        }
      }
    }

    class ContinuationNotifierWithCompletion<TResult> : ContinuationNotifier
    {
      readonly Completion<TResult> _comp;

      public ContinuationNotifierWithCompletion(ContinuationFunc<T, TResult> delg, Completion<TResult> comp)
        : base(delg) { this._comp = comp; }

      protected override void PerformContinuation(Delegate delg, Exception e, T res)
      {
        try
        {
          var rr = ((ContinuationFunc<T, TResult>)delg)(e, res);
          try
          {
            _comp.MarkCompleted(rr);
          }
          catch (Exception ee)
          {
            ContextFlow.NotifyUncaughtException(delg.Target, ee);
          }
        }
        catch (Exception ee)
        {
          try
          {
            _comp.MarkFaulted(ee);
          }
          catch (Exception eee)
          {
            ContextFlow.NotifyUncaughtException(delg.Target, eee);
          }
        }
      }
    }
  }
}