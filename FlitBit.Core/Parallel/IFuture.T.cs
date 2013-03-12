using System;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core.Parallel.CodeContracts;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   Interface for strongly typed future variables.
	/// </summary>
	/// <typeparam name="T">variable type T</typeparam>
	[ContractClass(typeof(ContractForIFuture<>))]
	public interface IFuture<T> : IFuture
	{
		/// <summary>
		///   Gets the future's value, blocking the current thread until it is available.
		/// </summary>
		/// <returns>the future's value</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		T Value { get; }

		/// <summary>
		///   Awaits the future's value.
		/// </summary>
		/// <returns>the variable's value.</returns>
		/// <exception cref="Exception">thrown if production of the future caused a fault.</exception>
		T AwaitValue();

		/// <summary>
		///   Waits for a period of time for the future's value.
		/// </summary>
		/// <param name="millisecondsTimeout">a timeout period in milliseconds.</param>
		/// <returns>the variable's value.</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		/// <exception cref="TimeoutException">thrown if the future has not completed in the timeout period.</exception>
		T AwaitValue(int millisecondsTimeout);

		/// <summary>
		///   Waits for a period of time for the future's value.
		/// </summary>
		/// <param name="timeout">a timeout period</param>
		/// <returns>the variable's value.</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		/// <exception cref="TimeoutException">thrown if the future has not completed in the timeout period.</exception>
		T AwaitValue(TimeSpan timeout);

		/// <summary>
		///   Tries to get the future's value, blocking the current thread until it is available.
		/// </summary>
		/// <param name="value">variable where the value will be returned.</param>
		/// <returns>true if the value is returned; otherwise false</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		bool TryGetValue(out T value);

		/// <summary>
		///   Tries to get the future's value, blocking the current thread for the timeout period.
		/// </summary>
		/// <param name="millisecondsTimeout">a timeout period in milliseconds.</param>
		/// <param name="value">variable where the value will be returned.</param>
		/// <returns>true if the value is returned; otherwise false</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		/// <exception cref="TimeoutException">thrown if the future has not completed in the timeout period.</exception>
		bool TryGetValue(int millisecondsTimeout, out T value);

		/// <summary>
		///   Tries to get the future's value, blocking the current thread for the timeout period.
		/// </summary>
		/// <param name="timeout">a timeout period</param>
		/// <param name="value">variable where the value will be returned.</param>
		/// <returns>true if the value is returned; otherwise false</returns>
		/// <exception cref="ParallelException">thrown if production of the future caused a fault.</exception>
		/// <exception cref="TimeoutException">thrown if the future has not completed in the timeout period.</exception>
		bool TryGetValue(TimeSpan timeout, out T value);

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="value"></param>
		void MarkCompleted(T value);

		/// <summary>
		///   Marks the completion.
		/// </summary>
		/// <param name="fault"></param>
		void MarkFaulted(Exception fault);
	}

	namespace CodeContracts
	{
		/// <summary>
		///   CodeContracts Class for IFuture&lt;>
		/// </summary>
		[ContractClassFor(typeof(IFuture<>))]
		internal abstract class ContractForIFuture<T> : IFuture<T>
		{
			public T AwaitValue()
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);

				throw new NotImplementedException();
			}

			public T AwaitValue(int millisecondsTimeout)
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);

				throw new NotImplementedException();
			}

			public T AwaitValue(TimeSpan timeout)
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);

				throw new NotImplementedException();
			}

			public bool TryGetValue(out T value) { throw new NotImplementedException(); }

			public bool TryGetValue(int millisecondsTimeout, out T value) { throw new NotImplementedException(); }

			public bool TryGetValue(TimeSpan timeout, out T value) { throw new NotImplementedException(); }

			public T Value
			{
				get { throw new NotImplementedException(); }
			}

			public void MarkCompleted(T value)
			{
				Contract.Requires<InvalidOperationException>(!IsCompleted);

				throw new NotImplementedException();
			}

			public void MarkFaulted(Exception fault)
			{
				Contract.Requires<InvalidOperationException>(!IsCompleted);
				Contract.Requires<ArgumentNullException>(fault != null);

				throw new NotImplementedException();
			}

			public Exception Exception
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsCompleted
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsFaulted
			{
				get { throw new NotImplementedException(); }
			}

			public object SyncObject
			{
				get { throw new NotImplementedException(); }
			}

			public bool Wait(TimeSpan timeout) { throw new NotImplementedException(); }

			public WaitHandle WaitHandle
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsDisposed
			{
				get { throw new NotImplementedException(); }
			}

			public void Dispose() { throw new NotImplementedException(); }
		}
	}
}