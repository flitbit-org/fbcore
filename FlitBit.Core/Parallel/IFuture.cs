using System;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core.Parallel.CodeContracts;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   Base interface for future variables.
	/// </summary>
	[ContractClass(typeof(ContractForIFuture))]
	public interface IFuture : IInterrogateDisposable
	{
		/// <summary>
		///   If faulted gets the associated exception.
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		///   Indicates whether the future is completed.
		/// </summary>
		bool IsCompleted { get; }

		/// <summary>
		///   Determines producing the future caused a fault.
		/// </summary>
		bool IsFaulted { get; }

		/// <summary>
		///   Gets the future's synchronization object.
		/// </summary>
		object SyncObject { get; }

		/// <summary>
		///   Gets a wait handle for the future.
		/// </summary>
		WaitHandle WaitHandle { get; }

		/// <summary>
		///   Waits for the future to be completed.
		/// </summary>
		/// <param name="timeout">the timeout period.</param>
		/// <returns>
		///   <em>true</em> if completed; otherwise <em>false</em>.
		/// </returns>
		bool Wait(TimeSpan timeout);
	}

	namespace CodeContracts
	{
		/// <summary>
		///   CodeContracts Class for IFuture&lt;>
		/// </summary>
		[ContractClassFor(typeof(IFuture))]
		internal abstract class ContractForIFuture : IFuture
		{
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

			public bool Wait(TimeSpan timeout)
			{
				Contract.Requires<ObjectDisposedException>(!IsDisposed);

				throw new NotImplementedException();
			}

			public WaitHandle WaitHandle
			{
				get
				{
					Contract.Requires<ObjectDisposedException>(!IsDisposed);

					throw new NotImplementedException();
				}
			}

			public bool IsDisposed
			{
				get { throw new NotImplementedException(); }
			}

			public void Dispose() { throw new NotImplementedException(); }
		}
	}
}