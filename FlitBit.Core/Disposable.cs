#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;

namespace FlitBit.Core
{
	/// <summary>
	/// Abstract logic for disposable objects.
	/// </summary>
	[Serializable]
	public abstract class Disposable : IDisposable
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
			this.Dispose(true);
			if (_disposal.CurrentState == DisposalState.Disposed)
			{
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Indicates whether the instance has been disposed.
		/// </summary>
		public bool IsDisposed { get { return _disposal.CurrentState == DisposalState.Disposed; } }

		void Dispose(bool disposing)
		{
			while (true)
			{
				var current = _disposal.CurrentState;
				if (current == DisposalState.Disposing)
				{
					if (!_disposal.TrySpinWaitForState(DisposalState.Incomplete, state => state == DisposalState.Disposed))
					{
						throw new ObjectDisposedException(this.GetType().FullName);
					}
				}

				if (_disposal.TryTransition(DisposalState.Disposing, DisposalState.Incomplete, DisposalState.None))
				{
					if (PerformDispose(disposing)) _disposal.ChangeState(DisposalState.Disposed);
					else _disposal.ChangeState(DisposalState.Incomplete);

					break;
				}
			}
		}

		/// <summary>
		/// Performs the dispose logic.
		/// </summary>
		/// <param name="disposing">Whether the object is disposing (IDisposable.Dispose method was called).</param>
		/// <returns>Implementers should return true if the disposal was successful; otherwise false.</returns>
		protected abstract bool PerformDispose(bool disposing);
	}

}
