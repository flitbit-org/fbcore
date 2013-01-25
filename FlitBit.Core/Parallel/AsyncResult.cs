#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;
using FlitBit.Core.Properties;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Basic implementation of the ITaskCompletion interface.
	/// </summary>
	public class AsyncResult : Disposable, IAsyncResult
	{
		const Int32 Status_Pending = 0;
		const Int32 Status_Completing = 1;
		const Int32 Status_CompletedSynchronously = 2;
		const Int32 Status_CompletedAsynchronously = 3;

		readonly AsyncCallback _asyncCallback;
		readonly Object _asyncHandback;
		
		int _completedState = Status_Pending;

		ManualResetEvent _waitHandle;
		Exception _fault;
		bool _disposed = false;
		
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		internal AsyncResult(ManualResetEvent evt)
			: this(null, null, null)
		{
			_waitHandle = evt;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public AsyncResult()
			: this(null, null, null)
		{
		}

		/// <summary>
		/// Creates a new instance and initializes the AsyncCallback.
		/// </summary>
		/// <param name="asyncCallback">A delegate to be called when the async operation completes.</param>
		/// <param name="asyncHandback">A handback object passed to the AsyncCallback when the operation completes.</param>
		public AsyncResult(AsyncCallback asyncCallback, Object asyncHandback)
			: this(asyncCallback, asyncHandback, null)
		{
		}

		/// <summary>
		/// Creates a new instance and initializes the AsyncCallback.
		/// </summary>
		/// <param name="asyncCallback">A delegate to be called when the async operation completes.</param>
		/// <param name="asyncHandback">A handback object passed to the AsyncCallback when the operation completes.</param>
		/// <param name="asyncState">A state object for use as a handback for the creator.</param>
		public AsyncResult(AsyncCallback asyncCallback, Object asyncHandback, Object asyncState)
		{
			_asyncCallback = asyncCallback;
			_asyncHandback = asyncHandback;
			this.AsyncState = asyncState;
		}
		
		#region IAsyncResult implementation

		/// <summary>
		/// Gets the task's asynchronous state.
		/// </summary>
		public Object AsyncState { get; private set; }

		/// <summary>
		/// Indicates whether the task completed synchronously.
		/// </summary>
		public bool CompletedSynchronously
		{
			get
			{
				return Thread.VolatileRead(ref _completedState) == Status_CompletedSynchronously;
			}
		}
		/// <summary>
		/// Gets the task's wait handle.
		/// </summary>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (_disposed) throw new ObjectDisposedException(this.GetType().GetReadableFullName());

				var handle = Util.VolatileRead(ref _waitHandle);
				if (handle == null)
				{
					Boolean done = IsCompleted;
					handle = new ManualResetEvent(done);
					if (Interlocked.CompareExchange(ref _waitHandle,
						 handle, null) != null)
					{
						// Another thread beat us too it, dispose the event...
						handle.Close();
					}
					else
					{
						if (!done && IsCompleted)
						{
							// If the operation was completed during signal creation, set the signal...							
							handle.Set();
						}
					}
				}
				return handle;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the asynchronous operation has completed.
		/// </summary>
		public bool IsCompleted { get { return Thread.VolatileRead(ref _completedState) > Status_Completing; } }

		#endregion

		/// <summary>
		/// Indicates whether the asynchronous operation resulted in a fault.
		/// </summary>
		public bool IsFaulted { get { return Util.VolatileRead(ref _fault) != null; } }

		/// <summary>
		/// Gets the exception that caused the fault.
		/// </summary>
		public Exception Exception { get { return _fault; } }

		internal void MarkCompleted(bool completedSynchronously)
		{
			PerformMarkCompleted(this, null, completedSynchronously);
		}

		internal void MarkException(Exception ex, bool completedSynchronously)
		{
			PerformMarkCompleted(this, self => { self._fault = ex; }, completedSynchronously);
		}

		/// <summary>
		/// Ends the asynchronous operation.
		/// </summary>
		public void EndInvoke()
		{
			if (!this.IsCompleted)
			{
				bool signalReceived = this.AsyncWaitHandle.WaitOne();
				// The following field refs are guaranteed by the getter above.
				_waitHandle.Close();
				Util.Dispose<ManualResetEvent>(ref _waitHandle);
				if (!signalReceived) throw new ParallelTimeoutException();
			}
			// If an exception occured, rethrow...
			if (_fault != null) throw _fault;
		}

		/// <summary>
		/// Ends the asynchronous operation.
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="exitContext"></param>
		public void EndInvoke(TimeSpan timeout, bool exitContext)
		{
			if (!this.IsCompleted)
			{
				bool signalReceived = this.AsyncWaitHandle.WaitOne(timeout, exitContext);
				// The following field refs are guaranteed by the getter above.
				_waitHandle.Close();
				Util.Dispose<ManualResetEvent>(ref _waitHandle);
				if (!signalReceived) throw new ParallelTimeoutException();
			}
			// If an exception occured, rethrow...
			if (_fault != null) throw _fault;
		}

		/// <summary>
		/// Ends the asynchronous operation.
		/// </summary>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="exitContext"></param>
		public void EndInvoke(int millisecondsTimeout, bool exitContext)
		{
			if (!this.IsCompleted)
			{
				bool signalReceived = this.AsyncWaitHandle.WaitOne(millisecondsTimeout, exitContext);
				// The following field refs are guaranteed by the getter above.
				_waitHandle.Close();
				Util.Dispose<ManualResetEvent>(ref _waitHandle);
				if (!signalReceived) throw new ParallelTimeoutException();
			}
			// If an exception occured, rethrow...
			if (_fault != null) throw _fault;
		}

		#region Utility methods
		/// <summary>
		/// Helper method for marking completions.
		/// </summary>
		/// <typeparam name="TSelf"></typeparam>
		/// <param name="self"></param>
		/// <param name="callback"></param>
		/// <param name="completedSynchronously"></param>
		protected static void PerformMarkCompleted<TSelf>(TSelf self, Action<TSelf> callback, bool completedSynchronously)
			where TSelf : AsyncResult
		{
			if (Interlocked.CompareExchange(ref self._completedState, Status_Completing, Status_Pending) != Status_Pending)
				throw new InvalidOperationException(Resources.Error_AsyncResultAlreadySet);

			if (callback != null) callback(self);
			var finalState = completedSynchronously ? Status_CompletedSynchronously : Status_CompletedAsynchronously;
			Interlocked.Exchange(ref self._completedState, finalState);

			var waitable = Util.VolatileRead(ref self._waitHandle);
			if (waitable != null)
			{
				waitable.Set();
			}
			if (self._asyncCallback != null) self._asyncCallback(self);
		}		
		#endregion

		/// <summary>
		/// Performs a disposal of the async result.
		/// </summary>
		/// <param name="disposing"></param>
		/// <returns></returns>
		protected override bool PerformDispose(bool disposing)
		{
			Util.Dispose(ref this._waitHandle);
			return true;
		}
	}
}
