using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// A set of continuations, each signaled once.
	/// </summary>
	public class ContinuationSet<T>
	{
		int _completed = 0;
		Exception _fault;
		T _res;

		class ContinuationNotifier
		{
			int _continued = 0;
			Delegate _delg;

			public ContinuationNotifier(Delegate delg)
			{
				this._delg = delg;
			}
			public void ContinueWith(ContextFlow context, Exception e, T res)
			{
				if (Interlocked.CompareExchange(ref _continued, 1, 0) == 0)
				{
					ThreadPool.QueueUserWorkItem(unused =>
					{
						if (context != null) ContextFlow.Ambient.Push(context);
						try
						{
							PerformContinuation(_delg, e, res);
						}
						finally
						{
							if (context != null) ContextFlow.Ambient.TryPop(context);
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
					Parallel.Go.NotifyUncaughtException(delg.Target, ee);
				}
			}			
		}
		class ContinuationNotifierWithCompletion : ContinuationNotifier
		{
			Completion _comp;

			public ContinuationNotifierWithCompletion(Continuation<T> delg, Completion comp)
				: base(delg)
			{
				this._comp = comp;
			}
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
						Parallel.Go.NotifyUncaughtException(delg.Target, ee);
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
						Parallel.Go.NotifyUncaughtException(delg.Target, eee);
					}
				}
			}
		}
		class ContinuationNotifierWithCompletion<R> : ContinuationNotifier
		{
			Completion<R> _comp;

			public ContinuationNotifierWithCompletion(ContinuationFunc<T, R> delg, Completion<R> comp)
				: base(delg)
			{
				this._comp = comp;
			}
			protected override void PerformContinuation(Delegate delg, Exception e, T res)
			{
				try
				{
					R rr = (R)((ContinuationFunc<T, R>)delg)(e, res);
					try
					{
						_comp.MarkCompleted(rr);
					}
					catch (Exception ee)
					{
						Parallel.Go.NotifyUncaughtException(delg.Target, ee);
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
						Parallel.Go.NotifyUncaughtException(delg.Target, eee);
					}
				}
			}
		}
		ConcurrentQueue<ContinuationNotifier> _continuations = new ConcurrentQueue<ContinuationNotifier>();
		readonly ContextFlow _context;

		internal ContinuationSet(ContextFlow context)
		{
			_context = context;
		}

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
					Go.Parallel(background_Notifier);
				}
			}
		}

		internal Completion ContinueWithCompletion(Continuation<T> continuation)
		{
			Completion waitable = new Completion(continuation.Target);
			try
			{
				_continuations.Enqueue(new ContinuationNotifierWithCompletion(continuation, waitable));
			}
			finally
			{
				if (Thread.VolatileRead(ref _completed) > 0)
				{
					Go.Parallel(background_Notifier);
				}
			}
			return waitable;
		}

		internal Completion<R> ContinueWithCompletion<R>(ContinuationFunc<T, R> continuation)
		{
			Completion<R> waitable = new Completion<R>(continuation.Target);
			try
			{
				_continuations.Enqueue(new ContinuationNotifierWithCompletion<R>(continuation, waitable));
			}
			finally
			{
				if (Thread.VolatileRead(ref _completed) > 0)
				{
					Go.Parallel(background_Notifier);
				}
			}
			return waitable;
		}

		internal void NotifyCompletion(Exception e, T res)
		{
			Util.VolatileWrite(ref _fault, e);
			Util.VolatileWrite(ref _res, res);
			Interlocked.Increment(ref _completed);
			Go.Parallel(background_Notifier);
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
	}
}
