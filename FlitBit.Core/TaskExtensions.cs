using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlitBit.Core
{
  /// <summary>
  ///   Contains task extensions.
  /// </summary>
  public static class TaskExtensions
  {
    // Strategy and starter code is from:
    // http://bradwilson.typepad.com/blog/2012/04/tpl-and-servers-pt3.html

    /// <summary>
    ///   Helper method that handles propagating the synchronization context to the
    ///   continuation when appropriate.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public static Task<TOut> Continue<TIn, TOut>(
      this Task<TIn> task,
      Func<Task<TIn>, TOut> continuation)
    {
      if (task.IsCompleted)
      {
        var tcs = new TaskCompletionSource<TOut>();

        try
        {
          var res = continuation(task);
          tcs.TrySetResult(res);
        }
        catch (Exception ex)
        {
          tcs.TrySetException(ex);
        }

        return tcs.Task;
      }

      return ContinueClosure(task, continuation);
    }

    static Task<TOut> ContinueClosure<TIn, TOut>(
      Task<TIn> task,
      Func<Task<TIn>, TOut> continuation)
    {
      var ctxt = SynchronizationContext.Current;

      return task.ContinueWith(innerTask =>
      {
        var tcs = new TaskCompletionSource<TOut>();

        try
        {
          if (ctxt != null)
          {
            ctxt.Post(state =>
            {
              try
              {
                var res = continuation(innerTask);
                tcs.TrySetResult(res);
              }
              catch (Exception ex)
              {
                tcs.TrySetException(ex);
              }
            }, null);
          }
          else
          {
            var res = continuation(innerTask);
            tcs.TrySetResult(res);
          }
        }
        catch (Exception ex)
        {
          tcs.TrySetException(ex);
        }

        return tcs.Task;
      }).Unwrap();
    }

    /// <summary>
    ///   Helper method that handles propagating the synchronization context to the
    ///   continuation when appropriate; transforms success and exceptions to the
    ///   output type TOut.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="continuation"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Task<TOut> ContinueTransform<TIn, TOut>(this Task<TIn> task,
      Func<Task<TIn>, TOut> continuation,
      Func<Exception, TOut> error)
    {
      if (task.IsCompleted)
      {
        var tcs = new TaskCompletionSource<TOut>();

        try
        {
          var res = continuation(task);
          tcs.TrySetResult(res);
        }
        catch (Exception ex)
        {
          tcs.TrySetResult(error(ex));
        }

        return tcs.Task;
      }

      return ContinueTransformClosure(task, continuation, error);
    }

    static Task<TOut> ContinueTransformClosure<TIn, TOut>(
      Task<TIn> task,
      Func<Task<TIn>, TOut> continuation,
      Func<Exception, TOut> error)
    {
      var ctxt = SynchronizationContext.Current;

      return task.ContinueWith(innerTask =>
      {
        var tcs = new TaskCompletionSource<TOut>();

        try
        {
          if (ctxt != null)
          {
            ctxt.Post(state =>
            {
              try
              {
                var res = continuation(innerTask);
                tcs.TrySetResult(res);
              }
              catch (Exception ex)
              {
                tcs.TrySetResult(error(ex));
              }
            }, null);
          }
          else
          {
            var res = continuation(innerTask);
            tcs.TrySetResult(res);
          }
        }
        catch (Exception ex)
        {
          tcs.TrySetResult(error(ex));
        }

        return tcs.Task;
      }).Unwrap();
    }
  }
}