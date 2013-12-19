using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using TPL = System.Threading.Tasks;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Indicates kind of results when de-multiplexing.
  /// </summary>
  public enum DemuxResultKind
  {
    /// <summary>
    ///   None.
    /// </summary>
    None = 0,

    /// <summary>
    ///   The result was observed. This indicates the current thread observed a result
    ///   originated by another thread.
    /// </summary>
    Observed = 1,

    /// <summary>
    ///   The result was originated by the current thread.
    /// </summary>
    Originated = 3,
  }
  
  /// <summary>
  /// Result of a demultiplexed operation.
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  public sealed class DemuxResult<TResult>
  {
    private readonly TResult _result;

    internal DemuxResult(DemuxResultKind kind, TResult result)
    {
      Kind = kind;
      _result = result;
    }

    internal DemuxResult(DemuxResultKind kind, Exception error)
    {
      Kind = kind;
      Error = error;
    } 

    /// <summary>
    /// Gets the result kind.
    /// </summary>
    public DemuxResultKind Kind { get; private set; }

    /// <summary>
    /// Gets the result of the demultiplexed operation.
    /// </summary>
    public TResult Result
    {
      get
      {
        if (Error != null)
          throw new ParallelException("An unexpected exception occurred during the background operation.", Error);
        return _result;
      }
    }

    /// <summary>
    /// Gets the backround exception if one occurred.
    /// </summary>
    public Exception Error { get; private set; }
  }

  /// <summary>
  ///   Produces a result type R, given an argument type A, demultiplexing concurrent requests.
  /// </summary>
  /// <typeparam name="TArgs">argument type A</typeparam>
  /// <typeparam name="TResult">result type R</typeparam>
  public abstract class DemuxProducer<TArgs, TResult>
  {
    class DemuxOpRecord
    {
      readonly Object _lock = new Object();
      readonly Queue<Action<DemuxResult<TResult>>> _actions =
        new Queue<Action<DemuxResult<TResult>>>();

      DemuxResult<TResult> _result = null;

      public void Subscribe(Action<DemuxResult<TResult>> consumer)
      {
        if (_result == null)
        {
          lock (_lock)
          {
            if (_result == null)
            {
              _actions.Enqueue(consumer);
              return;
            }
          }
        }
        consumer(_result);
      }

      public void Notify(DemuxResult<TResult> res)
      {
        Action<DemuxResult<TResult>>[] items = null;
        lock (_lock)
        {
          _result = res;
          if (_actions.Count > 0)
          {
            items = new Action<DemuxResult<TResult>>[_actions.Count];
            _actions.CopyTo(items, 0);
            _actions.Clear();
          }
        }
        if (items != null)
        {
          TPL.Parallel.ForEach(items, action => action(res));
        }
      }
    }
    readonly ConcurrentDictionary<TArgs, DemuxOpRecord> _concurrentActiviy =
      new ConcurrentDictionary<TArgs, DemuxOpRecord>();

    /// <summary>
    ///   Tries to demux a completion result.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="consumer">A continuation called upon completion.</param>
    public void TryConsume(TArgs args, Action<DemuxResult<TResult>> consumer)
    {
      Contract.Requires<ArgumentNullException>(consumer != null);
      DemuxTryConsume(args, consumer);
    }

    /// <summary>
    ///   Produces the requested result.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract bool ProduceResult(TArgs arg, out TResult value);

    void DemuxTryConsume(TArgs args, Action<DemuxResult<TResult>> consumer)
    {
      DemuxOpRecord capture = null;
      var local = this._concurrentActiviy.GetOrAdd(args, a =>
      {
        capture = new DemuxOpRecord();
        return capture;
      });

      if (ReferenceEquals(capture, local))
      {
        ThreadPool.QueueUserWorkItem(obj =>
        {
          try
          {
            var rec = (DemuxOpRecord)obj;
            try
            {
              TResult product;
              if (ProduceResult(args, out product))
              {
                consumer(new DemuxResult<TResult>(DemuxResultKind.Originated, product));
                rec.Notify(new DemuxResult<TResult>(DemuxResultKind.Observed, product));
              }
              else
              {
                var res = new DemuxResult<TResult>(DemuxResultKind.None, default(TResult));
                consumer(res);
                rec.Notify(res);
              }
            }
            catch (Exception e)
            {
              consumer(new DemuxResult<TResult>(DemuxResultKind.Originated, e));
              rec.Notify(new DemuxResult<TResult>(DemuxResultKind.Observed, e));
            }

          }
          finally
          {
            DemuxOpRecord unused;
            _concurrentActiviy.TryRemove(args, out unused);
          }
        }, local)
      ;
      }
      else
      {
        local.Subscribe(consumer);
      }
    }
  }
}