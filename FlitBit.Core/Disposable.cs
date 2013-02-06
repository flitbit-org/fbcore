#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace FlitBit.Core
{
	/// <summary>
	/// Abstract logic for disposable objects.
	/// </summary>
	[Serializable]
	public abstract class Disposable : IInterrogateDisposable
	{
		enum DisposalState
		{
			None = 0,
			Incomplete = 1,
			Disposing = 2,
			Disposed = 3
		}
		Status<DisposalState> _disposal = new Status<DisposalState>();

		/// <summary>
		/// Finalizes the instance.
		/// </summary>
		~Disposable()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Disposes the instance.
		/// </summary>
		public void Dispose()
		{
			if (Dispose(true))
			{
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Indicates whether the instance has been disposed.
		/// </summary>
		public bool IsDisposed { get { return _disposal.CurrentState == DisposalState.Disposed; } }

		bool Dispose(bool disposing)
		{
			Contract.Requires<ObjectDisposedException>(!this.IsDisposed);

			while (_disposal.IsLessThan(DisposalState.Disposed))
			{
				if (_disposal.HasState(DisposalState.Disposing))
				{
					if (!_disposal.TrySpinWaitForState(DisposalState.Incomplete, state => state == DisposalState.Disposed))
					{
						throw new ObjectDisposedException(this.GetType().FullName);
					}
				}
				else if (_disposal.TryTransition(DisposalState.Disposing, DisposalState.Incomplete, DisposalState.None))
				{
					if (PerformDispose(disposing))
					{
						if (_disposal.ChangeState(DisposalState.Disposed))
						{
							return true;
						}						
					}
					else
					{
						_disposal.ChangeState(DisposalState.Incomplete);
					}
					break;
				}
			}
			return false;
		}

		/// <summary>
		/// Performs the dispose logic.
		/// </summary>
		/// <param name="disposing">Whether the object is disposing (IDisposable.Dispose method was called).</param>
		/// <returns>Implementers should return true if the disposal was successful; otherwise false.</returns>
		protected abstract bool PerformDispose(bool disposing);
	}


}
