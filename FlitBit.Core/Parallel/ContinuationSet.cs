using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   A set of continuations, each signaled once.
	/// </summary>
	public class ContinuationSet
	{
		readonly ContextFlow _context;
		readonly ConcurrentQueue<ContinuationNotifier> _continuations = new ConcurrentQueue<ContinuationNotifier>();

		int _completed;
		Exception _fault;

		internal ContinuationSet(ContextFlow context) { _context = context; }

		internal void Continue(Continuation continuation)
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

		internal Completion ContinueWithCompletion(Continuation continuation)
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
					Go.Parallel(background_Notifier);
				}
			}
			return waitable;
		}

		internal Completion<TResult> ContinueWithCompletion<TResult>(ContinuationFunc<TResult> continuation)
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
					Go.Parallel(background_Notifier);
				}
			}
			return waitable;
		}

		internal void NotifyCompletion(Exception e)
		{
			Util.VolatileWrite(out _fault, e);
			Interlocked.Increment(ref _completed);
			Go.Parallel(background_Notifier);
		}

		void background_Notifier()
		{
			var e = Util.VolatileRead(ref _fault);
			ContinuationNotifier notifier;
			while (_continuations.TryDequeue(out notifier))
			{
				notifier.ContinueWith(_context, e);
			}
		}

		class ContinuationNotifier
		{
			readonly Delegate _delg;
			int _continued;

			public ContinuationNotifier(Delegate delg) { this._delg = delg; }

			public void ContinueWith(ContextFlow context, Exception e)
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
							PerformContinuation(_delg, e);
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

			protected virtual void PerformContinuation(Delegate delg, Exception e)
			{
				try
				{
					((Continuation) delg)(e);
				}
				catch (Exception ee)
				{
					Go.NotifyUncaughtException(delg.Target, ee);
				}
			}
		}

		class ContinuationNotifierWithCompletion : ContinuationNotifier
		{
			readonly Completion _comp;

			public ContinuationNotifierWithCompletion(Continuation delg, Completion comp)
				: base(delg) { this._comp = comp; }

			protected override void PerformContinuation(Delegate delg, Exception e)
			{
				try
				{
					((Continuation) delg)(e);
					try
					{
						_comp.MarkCompleted();
					}
					catch (Exception ee)
					{
						Go.NotifyUncaughtException(delg.Target, ee);
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
						Go.NotifyUncaughtException(delg.Target, eee);
					}
				}
			}
		}

		class ContinuationNotifierWithCompletion<TResult> : ContinuationNotifier
		{
			readonly Completion<TResult> _comp;

			public ContinuationNotifierWithCompletion(ContinuationFunc<TResult> delg, Completion<TResult> comp)
				: base(delg) { this._comp = comp; }

			protected override void PerformContinuation(Delegate delg, Exception e)
			{
				try
				{
					var res = ((ContinuationFunc<TResult>) delg)(e);
					try
					{
						_comp.MarkCompleted(res);
					}
					catch (Exception ee)
					{
						Go.NotifyUncaughtException(delg.Target, ee);
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
						Go.NotifyUncaughtException(delg.Target, eee);
					}
				}
			}
		}
	}
}