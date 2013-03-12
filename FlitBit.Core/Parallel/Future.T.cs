using System;
using System.Diagnostics;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   A Future variable of type T.
	/// </summary>
	/// <typeparam name="T">variable type T</typeparam>
	public sealed class Future<T> : Disposable, IFuture<T>
	{
		const int StatusWaiting = 0;
		const int StatusCompleted = 1;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Object _sync = new Object();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Exception _fault;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _status;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		T _value;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ManualResetEventSlim _waitable;

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		public Future() { _status = StatusWaiting; }

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		/// <param name="value">the future's value</param>
		public Future(T value)
		{
			_value = value;
			_status = StatusCompleted;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal ManualResetEventSlim Waitable
		{
			get
			{
				if (_waitable == null)
				{
					lock (_sync)
					{
						if (_waitable == null)
						{
							Util.VolatileWrite(ref _waitable, new ManualResetEventSlim(this.IsCompleted));
						}
					}
				}
				return _waitable;
			}
		}

		/// <summary>
		///   Gets the future's synchronization object.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public object SyncObject
		{
			get { return this._sync; }
		}

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="value"></param>
		public void MarkCompleted(T value)
		{
			lock (_sync)
			{
				_value = value;
				_status = StatusCompleted;
				if (_waitable != null)
				{
					_waitable.Set();
				}
				Monitor.PulseAll(_sync);
			}
		}

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="fault"></param>
		public void MarkFaulted(Exception fault)
		{
			lock (_sync)
			{
				_fault = fault;
				_status = StatusCompleted;
				if (_waitable != null)
				{
					_waitable.Set();
				}
				Monitor.PulseAll(_sync);
			}
		}

		/// <summary>
		///   Indicates whether the wait has completed.
		/// </summary>
		public bool IsCompleted
		{
			get { return Thread.VolatileRead(ref _status) == StatusCompleted; }
		}

		/// <summary>
		///   Determines if the completion resulted in an error.
		/// </summary>
		public bool IsFaulted
		{
			get { return Util.VolatileRead(ref _fault) != null; }
		}

		/// <summary>
		///   Gets the exception that caused the fault.
		/// </summary>
		public Exception Exception
		{
			get { return Util.VolatileRead(ref _fault); }
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>
		///   <em>true</em> if the value is present; otherwise <em>false</em>.
		/// </returns>
		public bool Wait(TimeSpan timeout)
		{
			if (timeout.Ticks > 0)
			{
				lock (_sync)
				{
					if (!this.IsCompleted)
					{
						Monitor.Wait(_sync, timeout);
					}
				}
			}
			return this.IsCompleted;
		}

		/// <summary>
		///   Gets the future variable's value. Warning! Reading this property
		///   will block your thread indefinitely or until the future variable
		///   has been set; whichever comes sooner.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public T Value
		{
			get { return AwaitValue(); }
		}

		/// <summary>
		///   Tries to read the value. This call will not block the calling
		///   thread if the value is not present.
		/// </summary>
		/// <param name="value">
		///   A reference where the value will be written if
		///   it is present.
		/// </param>
		/// <returns>
		///   <em>true</em> if the value was successfully read; otherwise <em>false</em>.
		/// </returns>
		public bool TryGetValue(out T value)
		{
			if (this.IsCompleted)
			{
				if (!this.IsFaulted)
				{
					value = this.Value;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		/// <summary>
		///   Tries to read the value. This call will not block the calling
		///   thread for the period of the timeout if the value is not present.
		/// </summary>
		/// <param name="millisecondsTimeout">timeout in milliseconds</param>
		/// <param name="value">
		///   A reference where the value will be written if
		///   it is present.
		/// </param>
		/// <returns>
		///   <em>true</em> if the value was successfully read; otherwise <em>false</em>.
		/// </returns>
		public bool TryGetValue(int millisecondsTimeout, out T value)
		{
			if (millisecondsTimeout > 0)
			{
				return TryGetValue(TimeSpan.FromMilliseconds(millisecondsTimeout), out value);
			}
			else
			{
				return TryGetValue(out value);
			}
		}

		/// <summary>
		///   Tries to read the value. This call will not block the calling
		///   thread for the period of the timeout if the value is not present.
		/// </summary>
		/// <param name="timeout">the timeout</param>
		/// <param name="value">
		///   A reference where the value will be written if
		///   it is present.
		/// </param>
		/// <returns>
		///   <em>true</em> if the value was successfully read; otherwise <em>false</em>.
		/// </returns>
		public bool TryGetValue(TimeSpan timeout, out T value)
		{
			if (Wait(timeout))
			{
				value = Value;
				return true;
			}
			value = default(T);
			return false;
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present.
		/// </summary>
		/// <returns>The future's value.</returns>
		public T AwaitValue()
		{
			lock (_sync)
			{
				if (!this.IsCompleted)
				{
					Monitor.Wait(_sync);
				}
			}

			if (this.IsFaulted)
			{
				throw new ParallelException("Background thread faulted.", this.Exception);
			}

			return Util.VolatileRead(ref _value);
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
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
			else if (this.IsCompleted)
			{
				if (this.IsFaulted)
				{
					throw new ParallelException("Background thread faulted.", this.Exception);
				}

				return Util.VolatileRead(ref _value);
			}

			throw new TimeoutException();
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>The future's value.</returns>
		/// <exception cref="TimeoutException">thrown if the timeout is exceeded before the value becomes available.</exception>
		public T AwaitValue(TimeSpan timeout)
		{
			if (Wait(timeout))
			{
				if (this.IsFaulted)
				{
					throw new ParallelException("Background thread faulted.", this.Exception);
				}

				return Util.VolatileRead(ref _value);
			}

			throw new TimeoutException();
		}

		/// <summary>
		///   Gets a wait handle for the future.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public WaitHandle WaitHandle
		{
			get { return Waitable.WaitHandle; }
		}

		/// <summary>
		///   Disposes the future and it's wait handle.
		/// </summary>
		/// <param name="disposing">indicates whether the object is disposing</param>
		/// <returns>if disposing, returns true if the disposal should continue.</returns>
		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				Util.Dispose(ref _waitable);
			}
			return disposing;
		}
	}
}