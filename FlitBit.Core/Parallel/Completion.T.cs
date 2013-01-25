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
	public sealed class CompletionEventArgs<T> : EventArgs
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="completion">the completion</param>
		public CompletionEventArgs(Completion<T> completion)
		{
			this.Completion = completion;
		}
		/// <summary>
		/// Gets the completion upon which the event fired.
		/// </summary>
		public Completion<T> Completion { get; private set; }
	}

	/// <summary>
	/// Default waitable implementation.
	/// </summary>
	public class Completion<T>
	{
		const int Status_Waiting = 0;
		const int Status_Completed = 1;

		class EventHelper : Disposable
		{
			readonly Completion<T> _owner;
			internal event EventHandler<CompletionEventArgs<T>> _completed;
			internal event EventHandler<CompletionEventArgs<T>> _faulted;
			int _status = Status_Waiting;
			ManualResetEventSlim _waitable;

			internal EventHelper(Completion<T> owner, Object target)
			{
				Contract.Requires<ArgumentNullException>(owner != null);

				_owner = owner;
				Target = target;
			}

			internal Object Target { get; private set; }
			internal bool IsCompleted { get { return Thread.VolatileRead(ref _status) == Status_Completed; } }

			internal ManualResetEventSlim Event
			{
				get
				{
					var waitable = Util.VolatileRead(ref _waitable);
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
					var args = new CompletionEventArgs<T>(_owner);
					var delegates = _completed.GetInvocationList();
					foreach (EventHandler<CompletionEventArgs<T>> delg in delegates)
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
					var args = new CompletionEventArgs<T>(_owner);
					var delegates = _faulted.GetInvocationList();
					foreach (EventHandler<CompletionEventArgs<T>> delg in delegates)
					{
						delg(Target, args);
						_completed -= delg;
					}
				}
			}

			private void SignalCompletedEvent()
			{
				var waitable = Util.VolatileRead(ref _waitable);
				if (waitable != null && !waitable.IsSet)
				{
					waitable.Set();
				}
			}

			protected override bool PerformDispose(bool disposing)
			{
				var waitable = Util.VolatileRead(ref _waitable);
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
		Object _lock;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _status;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Exception _fault;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		AsyncResult _asyncResult;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		T _value;

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		public Completion(Object target) 
		{
			_events = new EventHelper(this, target);
			_status = Status_Waiting;
		}

		/// <summary>
		/// Marks the completion.
		/// </summary>
		/// <param name="value"></param>
		public void MarkCompleted(T value)
		{
			PerformMarkCompleted(this, self => Util.VolatileWrite(ref _value, value), _events.NotifyCompleted);
		}

		/// <summary>
		/// Marks the completion.
		/// </summary>
		/// <param name="fault"></param>
		public void MarkFaulted(Exception fault)
		{
			PerformMarkCompleted(this, self => _fault = fault, _events.NotifyFaulted);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Object SyncRoot
		{
			get
			{
				var sync = Thread.VolatileRead(ref _lock);
				if (sync == null)
				{
					Boolean done = this.IsCompleted;
					sync = new Object();

					var prior = Interlocked.CompareExchange(ref _lock, sync, null);
					if (prior != null)
					{
						sync = prior;
					}
					else
					{
						if (!done && this.IsCompleted)
						{
							lock (sync)
							{
								// The value was set while creating sync root, signal it.
								Monitor.PulseAll(sync);
							}
						}
					}

				}
				return sync;
			}
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
		public Exception Exception { get { return _fault; } }

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
				var sync = this.SyncRoot;
				lock (sync)
				{
					remainingTicks = timeoutExpiration - DateTime.Now.Ticks;
					while (!this.IsFaulted && !this.IsCompleted && remainingTicks > 0)
					{
						if (!Monitor.Wait(sync, new TimeSpan(remainingTicks)))
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
		/// Gets the future variable's value. Warning! Reading this property
		/// will block your thread indefinitely or until the future variable
		/// has been set; whichever comes sooner.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public T Value { get { return AwaitValue(); } }

		/// <summary>
		/// Tries to read the value. This call will not block the calling
		/// thread if the value is not present.
		/// </summary>
		/// <param name="value">A reference where the value will be written if 
		/// it is present.</param>
		/// <returns><em>true</em> if the value was successfully read; otherwise <em>false</em>.</returns>
		public bool TryGetValue(out T value)
		{
			if (!this.IsFaulted && this.IsCompleted)
			{
				value = Util.VolatileRead(ref _value);
				return true;
			}

			value = default(T);
			return false;
		}

		/// <summary>
		/// Tries to read the value. This call will not block the calling
		/// thread for the period of the timeout if the value is not present.
		/// </summary>
		/// <param name="millisecondsTimeout">timeout in milliseconds</param>
		/// <param name="value">A reference where the value will be written if 
		/// it is present.</param>
		/// <returns><em>true</em> if the value was successfully read; otherwise <em>false</em>.</returns>
		public bool TryGetValue(int millisecondsTimeout, out T value)
		{
			if (millisecondsTimeout > 0)
			{
				return TryGetValue(TimeSpan.FromMilliseconds(millisecondsTimeout), out value);
			}
			else if (!this.IsFaulted && this.IsCompleted)
			{
				value = Util.VolatileRead(ref _value);
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		/// <summary>
		/// Tries to read the value. This call will not block the calling
		/// thread for the period of the timeout if the value is not present.
		/// </summary>
		/// <param name="timeout">the timeout</param>
		/// <param name="value">A reference where the value will be written if 
		/// it is present.</param>
		/// <returns><em>true</em> if the value was successfully read; otherwise <em>false</em>.</returns>
		public bool TryGetValue(TimeSpan timeout, out T value)
		{
			if (Wait(timeout))
			{
				value = Util.VolatileRead(ref _value);
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		/// <summary>
		/// Waits (blocks the current thread) until the value is present.
		/// </summary>
		/// <returns>The future's value.</returns>
		public T AwaitValue()
		{
			if (!this.IsFaulted && !this.IsCompleted)
			{
				var sync = this.SyncRoot;
				lock (sync)
				{
					while (!this.IsFaulted && !this.IsCompleted)
						Monitor.Wait(sync);
				}
			}
			if (this.IsFaulted)
				throw new ParallelException("Background thread faulted.", _fault);

			return _value;
		}

		/// <summary>
		/// Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
		/// <returns>The future's value.</returns>
		/// <exception cref="ParallelTimeoutException">thrown if the timeout is exceeded before the value becomes available.</exception>
		public T AwaitValue(int millisecondsTimeout)
		{
			if (millisecondsTimeout > 0)
			{
				return AwaitValue(TimeSpan.FromMilliseconds(millisecondsTimeout));
			}
			else if (!this.IsFaulted && this.IsCompleted)
			{
				return _value;
			}

			throw new TimeoutException();
		}

		/// <summary>
		/// Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>The future's value.</returns>
		/// <exception cref="TimeoutException">thrown if the timeout is exceeded before the value becomes available.</exception>
		public T AwaitValue(TimeSpan timeout)
		{
			if (Wait(timeout))
			{
				return _value;
			}

			throw new TimeoutException();
		}

		/// <summary>
		/// Event fired exactly once upon completion.
		/// </summary>
		public event EventHandler<CompletionEventArgs<T>> Completed
		{
			add
			{
				if (!this.IsCompleted)
				{
					var sync = this.SyncRoot;
					lock (sync)
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
					value(_events.Target, new CompletionEventArgs<T>(this));
				}
			}
			remove { _events._completed -= value; }
		}

		/// <summary>
		/// Event fired exactly once when a fault is encountered.
		/// </summary>
		public event EventHandler<CompletionEventArgs<T>> Faulted
		{
			add
			{
				if (!this.IsFaulted)
				{
					var sync = this.SyncRoot;
					lock (sync)
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
					value(_events.Target, new CompletionEventArgs<T>(this));
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
			return ToAsyncResult(asyncCallback, asyncHandback, null);
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
		public AsyncCallback MakeAsyncCallback<H>(H handback, Func<IAsyncResult, H, T> handler)
		{
			return new AsyncCallback(ar =>
			{
				try
				{
					MarkCompleted(handler(ar, handback));
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
		public Completion Continue(Continuation<T> continuation)
		{
			if (continuation == null) throw new ArgumentNullException("success");

			Completion waitable = new Completion(continuation.Target);
			var handler = new EventHandler<CompletionEventArgs<T>>((sender, evt) =>
			{
				var caught = evt.Completion.Exception;
				try
				{
					continuation(caught, _value);
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
		public Completion<R> Continue<R>(ContinuationFunc<T,R> continuation)
		{
			if (continuation == null) throw new ArgumentNullException("after");

			Completion<R> waitable = new Completion<R>(continuation.Target);
			var handler = new EventHandler<CompletionEventArgs<T>>((sender, evt) =>
			{
				var caught = evt.Completion.Exception;
				R result = default(R);
				try
				{
					result = continuation(caught, _value);
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
		/// Helper method for marking completions.
		/// </summary>
		/// <typeparam name="TSelf"></typeparam>
		/// <param name="self"></param>
		/// <param name="callback"></param>
		/// <param name="after">an action to run after the operation completes</param>
		protected static void PerformMarkCompleted<TSelf>(TSelf self, Action<TSelf> callback, Action after)
			where TSelf : Completion<T>
		{
			// Ensure the wait completes only once...
			if (Interlocked.CompareExchange(ref self._status, Status_Completed, Status_Waiting) != Status_Waiting)
				throw new InvalidOperationException("Already completed");

			if (callback != null) callback(self);

			var sync = self.SyncRoot;
			if (sync != null)
			{
				lock (sync)
				{
					Monitor.PulseAll(sync);
				}
			}			
			if (after != null) after();
		}
	}
}
