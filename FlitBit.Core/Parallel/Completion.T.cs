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
	///   Default waitable implementation.
	/// </summary>
	public class Completion<T> : Disposable
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ContinuationSet<T> _continuations;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		AsyncResult _asyncResult;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ContextFlow _context;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Future<T> _future = new Future<T>();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Object _target;

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		public Completion(Object target)
			: this(target, false, default(T)) { }

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		/// <param name="target">action's target</param>
		/// <param name="completed">
		///   Indicates whether the wait has already
		///   completed.
		/// </param>
		/// <param name="value">the completion value</param>
		public Completion(Object target, bool completed, T value)
		{
			_target = target;
			if (completed)
			{
				_future.MarkCompleted(value);
			}
			_context = ContextFlow.ForkAmbient();
			_continuations = new ContinuationSet<T>(_context);
		}

		/// <summary>
		///   Indicates whether the wait has completed.
		/// </summary>
		public bool IsCompleted
		{
			get { return _future.IsCompleted; }
		}

		/// <summary>
		///   Determines if the completion resulted in an error.
		/// </summary>
		public bool IsFaulted
		{
			get { return _future.IsFaulted; }
		}

		/// <summary>
		///   Gets the exception that caused the fault.
		/// </summary>
		public Exception Exception
		{
			get { return _future.Exception; }
		}

		/// <summary>
		///   Gets a wait handle for the completion.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public WaitHandle WaitHandle
		{
			get
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);

				return _future.WaitHandle;
			}
		}

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="value">the completion value</param>
		public void MarkCompleted(T value)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);
			_future.MarkCompleted(value);
			_continuations.NotifyCompletion(null, value);
		}

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="fault"></param>
		public void MarkFaulted(Exception fault)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);
			_future.MarkFaulted(fault);
			_continuations.NotifyCompletion(fault, default(T));
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>The future's value.</returns>
		public bool Wait(TimeSpan timeout) { return _future.Wait(timeout); }

		/// <summary>
		///   Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <returns></returns>
		public AsyncResult ToAsyncResult()
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);
			return ToAsyncResult(null, null, null);
		}

		/// <summary>
		///   Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <param name="asyncCallback"></param>
		/// <param name="asyncHandback"></param>
		/// <returns></returns>
		public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);
			return ToAsyncResult(asyncCallback, asyncHandback, null);
		}

		/// <summary>
		///   Gets an async result for .NET framework synchronization.
		/// </summary>
		/// <param name="asyncCallback"></param>
		/// <param name="asyncHandback"></param>
		/// <param name="asyncState"></param>
		/// <returns></returns>
		public AsyncResult ToAsyncResult(AsyncCallback asyncCallback, Object asyncHandback, Object asyncState)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);
			if (_asyncResult == null)
			{
				lock (_future.SyncObject)
				{
					if (_asyncResult == null)
					{
						_asyncResult = new AsyncResult(asyncCallback, asyncHandback, asyncState);
						this.Continue((e, res) =>
							{
								if (e != null)
								{
									_asyncResult.MarkException(e, false);
								}
								else
								{
									_asyncResult.MarkCompleted(false);
								}
							});
					}
				}
			}
			return _asyncResult;
		}

		/// <summary>
		///   Makes an AsyncCallback delegate that produces the completion.
		/// </summary>
		/// <typeparam name="H">handback type H</typeparam>
		/// <param name="handback">the handback</param>
		/// <param name="handler">a handler that produces the completion</param>
		/// <returns>An AsyncCallback.</returns>
		public AsyncCallback MakeAsyncCallback<H>(H handback, Func<IAsyncResult, H, T> handler)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);

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
		///   Waits (blocks the current thread) until the value is present.
		/// </summary>
		/// <returns>The future's value.</returns>
		public T AwaitValue()
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);

			return _future.AwaitValue();
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
		/// <returns>The future's value.</returns>
		/// <exception cref="ParallelTimeoutException">thrown if the timeout is exceeded before the value becomes available.</exception>
		public T AwaitValue(int millisecondsTimeout)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);

			return _future.AwaitValue(millisecondsTimeout);
		}

		/// <summary>
		///   Waits (blocks the current thread) until the value is present or the timeout is exceeded.
		/// </summary>
		/// <param name="timeout">A timespan representing the timeout period.</param>
		/// <returns>The future's value.</returns>
		/// <exception cref="TimeoutException">thrown if the timeout is exceeded before the value becomes available.</exception>
		public T AwaitValue(TimeSpan timeout)
		{
			Contract.Requires<ObjectDisposedException>(!IsDisposed);

			return _future.AwaitValue(timeout);
		}

		/// <summary>
		///   Schedules an action to execute when this completion is done.
		/// </summary>
		/// <param name="continuation">a continuation to run upon completion.</param>
		public void Continue(Continuation<T> continuation)
		{
			Contract.Requires<ArgumentNullException>(continuation != null);
			_continuations.Continue(continuation);
		}

		/// <summary>
		///   Schedules an action to execute when another completion succeeds.
		/// </summary>
		/// <param name="continuation">an action to run when the completion succeeds</param>
		/// <returns>a completion for the success action</returns>
		public Completion ContinueWithCompletion(Continuation<T> continuation)
		{
			Contract.Requires<ArgumentNullException>(continuation != null);
			Contract.Ensures(Contract.Result<Completion>() != null);
			return _continuations.ContinueWithCompletion(continuation);
		}

		/// <summary>
		///   Schedules a function to execute when another completion succeeds.
		/// </summary>
		/// <typeparam name="R">result type R</typeparam>
		/// <param name="continuation">a function to run when the completion succeeds</param>
		/// <returns>a completion for the success function</returns>
		public Completion<R> ContinueWithCompletion<R>(ContinuationFunc<T, R> continuation)
		{
			Contract.Requires<ArgumentNullException>(continuation != null);
			Contract.Ensures(Contract.Result<Completion<R>>() != null);
			return _continuations.ContinueWithCompletion(continuation);
		}

		/// <summary>
		///   Performs dispose on the completion.
		/// </summary>
		/// <param name="disposing"></param>
		/// <returns></returns>
		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				Util.Dispose(ref _future);
				Util.Dispose(ref _context);
			}
			return true;
		}
	}
}