using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

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
	///   Produces a result type R, given an argument type A, demultiplexing concurrent requests.
	/// </summary>
	/// <typeparam name="TArgs">argument type A</typeparam>
	/// <typeparam name="TResult">result type R</typeparam>
	public abstract class DemuxProducer<TArgs, TResult>
	{
		readonly ConcurrentDictionary<TArgs, Completion<Tuple<DemuxResultKind, TResult>>> _concurrentActiviy =
			new ConcurrentDictionary<TArgs, Completion<Tuple<DemuxResultKind, TResult>>>();

		/// <summary>
		///   Tries to demux a completion result.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="consumer">A continuation called upon completion.</param>
		public void TryConsume(TArgs args, Continuation<Tuple<DemuxResultKind, TResult>> consumer)
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

		void DemuxTryConsume(TArgs args, Continuation<Tuple<DemuxResultKind, TResult>> continuation)
		{
			Completion<Tuple<DemuxResultKind, TResult>> capture = null;
			var completion = this._concurrentActiviy.GetOrAdd(args, a =>
			{
				capture = new Completion<Tuple<DemuxResultKind, TResult>>(this);
				return capture;
			});

			if (ReferenceEquals(capture, completion))
			{
				Go.Parallel(() => PerformDemuxOriginatorLogic(args, completion, continuation));
			}
			else
			{
				completion.Continue(continuation);
			}
		}

		void PerformDemuxOriginatorLogic(TArgs args, Completion<Tuple<DemuxResultKind, TResult>> completion,
			Continuation<Tuple<DemuxResultKind, TResult>> continuation)
		{
			try
			{
				bool valueProduced;
				TResult value;
				try
				{
					valueProduced = ProduceResult(args, out value);
				}
				catch (Exception e)
				{
					completion.MarkFaulted(e);
					continuation(e, null);
					return;
				}
				if (valueProduced)
				{
					completion.MarkCompleted(new Tuple<DemuxResultKind, TResult>(DemuxResultKind.Observed, value));
					continuation(null, new Tuple<DemuxResultKind, TResult>(DemuxResultKind.Originated, value));
				}
				else
				{
					var res = new Tuple<DemuxResultKind, TResult>(DemuxResultKind.None, default(TResult));
					completion.MarkCompleted(new Tuple<DemuxResultKind, TResult>(DemuxResultKind.Observed, value));
					continuation(null, res);
				}
			}
			finally
			{
				Completion<Tuple<DemuxResultKind, TResult>> unused;
				_concurrentActiviy.TryRemove(args, out unused);
				completion.Dispose();
			}
		}
	}
}