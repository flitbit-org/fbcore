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
	/// Completion event arguments.
	/// </summary>
	public sealed class CompletionEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="completion">the completion</param>
		public CompletionEventArgs(Completion completion)
		{
			this.Completion = completion;
		}
		/// <summary>
		/// The completion upon which the event fired.
		/// </summary>
		public Completion Completion { get; private set; }
	}

	/// <summary>
	/// Default waitable implementation.
	/// </summary>
	public class Completion: Disposable
	{
		const int Status_Waiting = 0;
		const int Status_Completed = 1;

		class EventHelper : Disposable
		{
			readonly Completion _owner;
			internal event EventHandler<CompletionEventArgs> _completed;
			internal event EventHandler<CompletionEventArgs> _faulted;
			int _status = Status_Waiting;
			ManualResetEventSlim _waitable;

			internal EventHelper(Completion owner, Object target)
			{
				Contract.Requires<ArgumentNullException>(owner != null);

				_owner = owner;
				Target = target;
			}

			internal Object Target { get; private set; }

			internal bool IsCompleted
			{
				get { return Thread.VolatileRead(ref _status) == Status_Completed; }
			}

			internal ManualResetEventSlim Event
			{
				get
				{
					Thread.MemoryBarrier();
					var waitable = _waitable;
					Thread.MemoryBarrier();

					if (waitable == null)
					{
						waitable = new ManualResetEventSlim(this.IsCompleted);
						var theirs = Interlocked.CompareExchange(ref _waitable, waitable, null);
						if (theirs != null)
						{
							waitable.Dispose();
							waitable = theirs;
						}
					}
					return waitable;
				}
			}

			internal void NotifyCompleted()
			{
				if (_completed != null)
				{
					if (Interlocked.CompareExchange(ref _status, Status_Completed, Status_Waiting) == Status_Waiting)
					{
						SignalCompletedEvent();
					}
					// Ensure notifications only occur once...
					var args = new CompletionEventArgs(_owner);
					var delegates = _completed.GetInvocationList();
					foreach (EventHandler<CompletionEventArgs> delg in delegates)
					{
						delg(Target, args);
						_completed -= delg;
					}
				}
			}

			internal void NotifyFaulted()
			{
				if (_faulted != null)
				{
					if (Interlocked.CompareExchange(ref _status, Status_Completed, Status_Waiting) == Status_Waiting)
					{
						SignalCompletedEvent();
					}
					// Ensure notifications only occur once...
					var args = new CompletionEventArgs(_owner);
					var delegates = _faulted.GetInvocationList();
					foreach (EventHandler<CompletionEventArgs> delg in delegates)
					{
						delg(Target, args);
						_completed -= delg;
					}
				}
			}

			private void SignalCompletedEvent()
			{
				Thread.MemoryBarrier();
				var waitable = _waitable;
				Thread.MemoryBarrier();
				if (waitable != null && !waitable.IsSet)
				{
					waitable.Set();
				}
			}

			protected override bool PerformDispose(bool disposing)
			{
				Thread.MemoryBarrier();
				var waitable = _waitable;
				Thread.MemoryBarrier();

				if (waitable != null)
				{
					waitable.Dispose();
				}
				return true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		EventHelper _events;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Object _lock = new Object();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _status;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Exception _fault;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		AsyncResult _asyncResult;

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		public Completion(Object target) : this(target, false) { }

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		/// <param name="target">action's target</param>
		/// <param name="completed">Indicates whether the wait has already
		/// completed.</param>
		public Completion(Object target, bool completed)
		{
			_events = new EventHelper(this, target);
			_status = (completed) ? Status_Completed : Status_Waiting;
		}

		/// <summary>
		/// Marks the completion.
		/// </summary>
		public void MarkCompleted()
		{
			// Ensure the wait completes only once...
			var state = Thread.VolatileRead(ref _status);
			if (_status != Status_Waiting)
				throw new InvalidOperationException("Already completed");

			_status = Status_Completed;

			lock (_lock)
			{
				Monitor.PulseAll(_lock);
			}
			_events.NotifyCompleted();
		}

		/// <summary>
		/// Marks the completion.
		/// </summary>
		/// <param name="fault"></param>
		public void MarkFaulted(Exception fault)
		{
			// Ensure the wait completes only once...
			var state = Thread.VolatileRead(ref _status);
			if (_status != Status_Waiting)
				throw new InvalidOperationException("Already completed");
			
			Thread.MemoryBarrier();
			_fault = fault;
			_status = Status_Completed;
			Thread.MemoryBarrier();

			lock (_lock)
			{
				Monitor.PulseAll(_lock);
			}
			_events.NotifyFaulted();
		}
		
		/// <summary>
		/// Indicates whether the wait has completed.
		/// </summary>
		public bool IsCompleted { get { return Thread.VolatileRead(ref _status) == Status_Completed; } }

		/// <summary>
		/// Determines if the completion resulted in an error.
		/// </summary>
		public bool IsFaulted { get { return Util.VolatileRead(ref _fault) != null; } }

		/// <summary>
		/// Gets the exception that caused the fault.
		/// </summary>
		public Exception Exception  { get { return Util.VolatileRead(ref _fault); } }
		
		/// <summary>
		/// Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>The future's value.</returns>
		public bool Wait(TimeSpan timeout)
		{
			long ticks = DateTime.Now.Ticks;
			long timeoutExpiration = ticks + timeout.Ticks;
			long remainingTicks;

			if (!this.IsFaulted && !this.IsCompleted)
			{
				lock (_lock)
				{
					remainingTicks = timeoutExpiration - DateTime.Now.Ticks;
					while (!this.IsFaulted && !this.IsCompleted && remainingTicks > 0)
					{
						if (!Monitor.Wait(_lock, new TimeSpan(remainingTicks)))
						{
							return false;
						}
						remainingTicks = timeoutExpiration - DateTime.Now.Ticks;
					};
				}
			}
			return !this.IsFaulted && this.IsCompleted;
		}

		/// <summary>
		/// Event fired exactly once upon completion.
		/// </summary>
		public event EventHandler<CompletionEventArgs> Completed
		{
			add
			{
				if (!this.IsCompleted)
				{
					lock (_lock)
					{
						if (!this.IsCompleted)
						{
							_events._completed += value;
							return;
						}
					}
				}
				if (!this.IsFaulted)
				{
					// If we get here then fire the event immediately
					// because we're already completed.
					value(_events.Target, new CompletionEventArgs(this));
				}
			}
			remove { _events._completed -= value; }
		}

		/// <summary>
		/// Event fired exactly once when a fault is encountered.
		/// </summary>
		public event EventHandler<CompletionEventArgs> Faulted
		{
			add
			{
				if (!this.IsFaulted)
				{
					lock (_lock)
					{
						if (!this.IsFaulted && !this.IsCompleted)
						{
							_events._faulted += value;
							return;
						}
					}
				}
				if (this.IsFaulted)
				{
					// If we get here then fire the event immediately
					// because we're already faulted.
					value(_events.Target, new CompletionEventArgs(this));
				}
			}
			remove { _events._faulted -= value; }
		}

		/// <summary>
		/// Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <returns></returns>
		public AsyncResult ToAsyncResult()
		{
			return ToAsyncResult(null, null, null);
		}

		/// <summary>
		/// Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <param name="asyncCallback"></param>
		/// <param name="asyncHandback"></param>
		/// <returns></returns>
		public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback)
		{
			return ToAsyncResult(null, null, null);
		}

		/// <summary>
		/// Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <param name="asyncCallback"></param>
		/// <param name="asyncHandback"></param>
		/// <param name="asyncState"></param>
		/// <returns></returns>
		public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback, Object asyncState)
		{
			var async = Util.VolatileRead(ref _asyncResult);
			if (async == null)
			{
				async = new AsyncResult(asyncCallback, asyncHandback, asyncState);
				var theirs = Interlocked.CompareExchange(ref _asyncResult, async, null);
				if (theirs == null)
				{
					Completed += (sender, e) =>
					{
						async.MarkCompleted(false);
					};
					Faulted += (sender, e) =>
					{
						async.MarkException(e.Completion.Exception, false);
					};
				}
				else
				{
					async.Dispose();
					async = theirs;
				}
			}
			return async;
		}

		/// <summary>
		/// Makes an AsyncCallback delegate that produces the completion.
		/// </summary>
		/// <typeparam name="H">handback type H</typeparam>
		/// <param name="handback">the handback</param>
		/// <param name="handler">a handler that produces the completion</param>
		/// <returns>An AsyncCallback.</returns>
		public AsyncCallback MakeAsyncCallback<H>(H handback, Action<IAsyncResult, H> handler)
		{
			return new AsyncCallback(ar =>
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
			});
		}

		/// <summary>
		/// Gets a wait handle for the completion.
		/// </summary>
		public WaitHandle WaitHandle
		{
			get { return _events.Event.WaitHandle; }
		}

		/// <summary>
		/// Schedules an action to execute when another completion succeeds.
		/// </summary>
		/// <param name="continuation">an action to run when the completion succeeds</param>
		/// <returns>a completion for the success action</returns>
		public Completion Continue(Continuation continuation)
		{
			if (continuation == null) throw new ArgumentNullException("success");

			Completion waitable = new Completion(continuation.Target);
			var handler = new EventHandler<CompletionEventArgs>((sender, evt) =>
			{
				var caught = evt.Completion.Exception;
				try
				{
					continuation(caught);
				}
				catch (Exception e)
				{
					caught = e;
					try
					{
						waitable.MarkFaulted(caught);
					}
					catch (Exception ee)
					{
						Parallel.Go.NotifyUncaughtException(continuation.Target, ee);
					}
					return;
				}
				try
				{
					waitable.MarkCompleted();
				}
				catch (Exception ee)
				{
					Parallel.Go.NotifyUncaughtException(continuation.Target, ee);
				}
			});
			this.Completed += handler;
			this.Faulted += handler;
			return waitable;
		}

		/// <summary>
		/// Schedules a function to execute when another completion succeeds.
		/// </summary>
		/// <typeparam name="R">result type R</typeparam>
		/// <param name="continuation">a function to run when the completion succeeds</param>
		/// <returns>a completion for the success function</returns>
		public Completion<R> Continue<R>(ContinuationFunc<R> continuation)
		{
			if (continuation == null) throw new ArgumentNullException("after");

			Completion<R> waitable = new Completion<R>(continuation.Target);
			var handler = new EventHandler<CompletionEventArgs>((sender, evt) =>
			{
				var caught = evt.Completion.Exception;
				R result = default(R);
				try
				{
					result = continuation(caught);
				}
				catch (Exception e)
				{
					caught = e;
					try
					{
						waitable.MarkFaulted(caught);
					}
					catch (Exception ee)
					{
						Parallel.Go.NotifyUncaughtException(continuation.Target, ee);
					}
					return;
				}
				try
				{
					if (!waitable.IsFaulted)
					{
						waitable.MarkCompleted(result);
					}
				}
				catch (Exception ee)
				{
					Parallel.Go.NotifyUncaughtException(continuation.Target, ee);
				}
			});
			this.Completed += handler;
			this.Faulted += handler;

			return waitable;
		}
				
		/// <summary>
		/// Performs dispose on the completion.
		/// </summary>
		/// <param name="disposing"></param>
		/// <returns></returns>
		protected override bool PerformDispose(bool disposing)
		{
			Util.Dispose(ref _events);
			return true;
		}
	}
}