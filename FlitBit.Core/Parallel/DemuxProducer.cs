using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Indicates kind of results when de-multiplexing.
	/// </summary>
	public enum DemuxResultKind
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// The result was observed. This indicates the current thread observed a result
		/// originated by another thread.
		/// </summary>
		Observed = 1,
		/// <summary>
		/// The result was originated by the current thread.
		/// </summary>
		Originated = 3,
	}

	/// <summary>
	/// Produces a result type R, given an argument type A, demultiplexing concurrent requests.
	/// </summary>
	/// <typeparam name="A">argument type A</typeparam>
	/// <typeparam name="R">result type R</typeparam>
	public abstract class DemuxProducer<A, R>
	{
		readonly ConcurrentDictionary<A, Completion<Tuple<DemuxResultKind, R>>> _concurrentActiviy = new ConcurrentDictionary<A, Completion<Tuple<DemuxResultKind, R>>>();

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public DemuxProducer()
		{
		}

		/// <summary>
		/// Tries to demux a completion result.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="consumer">A continuation called upon completion.</param>
		public void TryConsume(A args, Continuation<Tuple<DemuxResultKind,R>> consumer)
		{
			Contract.Requires<ArgumentNullException>(consumer != null);
			DemuxTryConsume(args, consumer);
		}

		void DemuxTryConsume(A args, Continuation<Tuple<DemuxResultKind, R>> continuation)
		{
			Completion<Tuple<DemuxResultKind, R>> completion = null, capture = null;
			completion = _concurrentActiviy.GetOrAdd(args, a =>
			{
				capture = new Completion<Tuple<DemuxResultKind, R>>(this);
				return capture;
			});

			if (Object.ReferenceEquals(capture, completion))
			{
				ThreadPool.QueueUserWorkItem(unused =>
				{
					PerformDemuxOriginatorLogic(args, completion, continuation);
				});
			}
			else
			{
				completion.Continue(continuation);			
			}
		}

		private void PerformDemuxOriginatorLogic(A args, Completion<Tuple<DemuxResultKind, R>> completion, Continuation<Tuple<DemuxResultKind, R>> continuation)
		{
			var res = default(Tuple<DemuxResultKind, R>);
			try
			{
				bool valueProduced;
				R value;
				try
				{
					valueProduced = ProduceResult(args, out value);
				}
				catch (Exception e)
				{
					completion.MarkFaulted(e);
					continuation(e, res);
					return;
				}
				if (valueProduced)
				{
					completion.MarkCompleted(new Tuple<DemuxResultKind, R>(DemuxResultKind.Observed, value));
					continuation(null, new Tuple<DemuxResultKind, R>(DemuxResultKind.Originated, value));
				}
				else
				{
					res = new Tuple<DemuxResultKind, R>(DemuxResultKind.None, default(R));
					completion.MarkCompleted(new Tuple<DemuxResultKind, R>(DemuxResultKind.Observed, value));
					continuation(null, res);
				}			
			}
			finally
			{	
				Completion<Tuple<DemuxResultKind, R>> unused;
				_concurrentActiviy.TryRemove(args, out unused);
				completion.Dispose();
			}
		}
	

		/// <summary>
		/// Produces the requested result.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected abstract bool ProduceResult(A arg, out R value);
	}

}
