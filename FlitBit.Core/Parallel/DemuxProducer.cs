#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FlitBit.Core.Parallel
{
  
  /// <summary>
  ///   Produces a result type R, given an argument type A, demultiplexing concurrent requests.
  /// </summary>
  /// <typeparam name="TArgs">argument type A</typeparam>
  /// <typeparam name="TResult">result type R</typeparam>
  public abstract class DemuxProducer<TArgs, TResult>
  {
    readonly ConcurrentDictionary<TArgs, TaskCompletionSource<TResult>> _concurrentActiviy =
      new ConcurrentDictionary<TArgs, TaskCompletionSource<TResult>>();

    /// <summary>
    ///   Produces the requested result.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract bool ProduceResult(TArgs arg, out TResult value);

    /// <summary>
    /// Consumes an product asynchronously, demultiplexing concurrent requests.
    /// </summary>
    /// <param name="args">args identifying the product.</param>
    public Task<TResult> ConsumeAsync(TArgs args)
    {
      TaskCompletionSource<TResult> capture = null;
      var local = this._concurrentActiviy.GetOrAdd(args, a =>
      {
        capture = new TaskCompletionSource<TResult>();
        return capture;
      });

      if (ReferenceEquals(capture, local))
      {
        ThreadPool.QueueUserWorkItem(obj =>
        {
          var completion = (TaskCompletionSource<TResult>)obj;
          try
          {
            TResult product;
            ProduceResult(args, out product);
            completion.SetResult(product);
          }
          catch (OperationCanceledException)
          {
            completion.TrySetCanceled();
          }
          catch (Exception exc)
          {
            completion.TrySetException(exc);
          }
          finally
          {
            TaskCompletionSource<TResult> unused;
            _concurrentActiviy.TryRemove(args, out unused);
          }
        }, local);
      }
      return local.Task;
    }
  }
}