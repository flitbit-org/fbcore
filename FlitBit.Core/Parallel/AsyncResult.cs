﻿#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;
using FlitBit.Core.Properties;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Basic implementation of the ITaskCompletion interface.
	/// </summary>
	public class AsyncResult : Disposable, IAsyncResult
	{			
		readonly AsyncCallback _asyncCallback;
		readonly Object _asyncHandback;

		bool _completedSynchronously;		
		IFuture<bool> _future;
		
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		internal AsyncResult(IFuture<bool> future)
			: this(null, null, null)
		{
			Contract.Requires<ArgumentNullException>(future != null);
			_future = future;
			_completedSynchronously = future.IsCompleted;
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
			_future = new Future<bool>();
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
			get { return _completedSynchronously; }
		}
		/// <summary>
		/// Gets the task's wait handle.
		/// </summary>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				return _future.WaitHandle;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the asynchronous operation has completed.
		/// </summary>
		public bool IsCompleted
		{
			get
			{
				return _future.IsCompleted;
			}
		}

		#endregion

		/// <summary>
		/// Indicates whether the asynchronous operation resulted in a fault.
		/// </summary>
		public bool IsFaulted
		{
			get
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);
				return _future.IsFaulted;
			}
		}

		/// <summary>
		/// Gets the exception that caused the fault.
		/// </summary>
		public Exception Exception
		{
			get
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);
				return _future.Exception;
			}
		}

		internal void MarkCompleted(bool completedSynchronously)
		{
			Contract.Requires<InvalidOperationException>(!IsCompleted);

			_completedSynchronously = completedSynchronously;
			_future.MarkCompleted(true);

			Thread.MemoryBarrier();
			var callback = _asyncCallback;
			Thread.MemoryBarrier();
			if (callback != null) callback(this);
		}

		internal void MarkException(Exception ex, bool completedSynchronously)
		{
			Contract.Requires<InvalidOperationException>(!IsCompleted);

			_completedSynchronously = completedSynchronously;
			_future.MarkFaulted(ex);

			Thread.MemoryBarrier();
			var callback = _asyncCallback;
			Thread.MemoryBarrier();
			if (callback != null) callback(this);
		}

		/// <summary>
		/// Ends the asynchronous operation.
		/// </summary>
		public void EndInvoke()
		{
			if (!this.IsCompleted)
			{
				bool signalReceived = this.AsyncWaitHandle.WaitOne();
				if (!signalReceived) throw new ParallelException("Not completed");
			}
			// If an exception occured, rethrow...
			if (_future.IsFaulted)
			{
				throw new ParallelException(Resources.Err_ExceptionOccurredInParallelThread, _future.Exception);
			}
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
				if (!signalReceived) throw new ParallelException("Not completed");
			}
			// If an exception occured, rethrow...
			if (_future.IsFaulted)
			{
				throw new ParallelException(Resources.Err_ExceptionOccurredInParallelThread, _future.Exception);
			}
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
				if (!signalReceived) throw new ParallelException("Not completed");
			}
			// If an exception occured, rethrow...
			if (_future.IsFaulted)
			{
				throw new ParallelException(Resources.Err_ExceptionOccurredInParallelThread, _future.Exception);
			}
		}

		/// <summary>
		/// Performs a disposal of the async result.
		/// </summary>
		/// <param name="disposing"></param>
		/// <returns></returns>
		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				Util.Dispose(ref _future);
			}
			return true;
		}
	}
}
