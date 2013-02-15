#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Diagnostics;

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

#if DEBUG
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public Disposable()
		{
			this.CreationStack = new System.Diagnostics.StackTrace().GetFrames();
		}

		/// <summary>
		/// Exposes the call stack at the time of creation (DEBUG).
		/// </summary>
		public System.Diagnostics.StackFrame[] CreationStack { get; private set; }
#endif		

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
		/// Checks whether the class should trace events of <paramref name="eventType"/>.
		/// </summary>
		/// <param name="eventType">an event type</param>
		/// <returns><em>true</em> if the event type should be traced; otherwise <em>false</em>.</returns>
		protected virtual bool ShouldTrace(TraceEventType eventType)
		{
				return false;
		}

		/// <summary>
		/// Trace event sink. Should be specialized by subclasses to record trace events.
		/// </summary>
		/// <param name="eventType">an event type</param>
		/// <param name="message">a trace message</param>
		protected virtual void OnTraceEvent(TraceEventType eventType, string message)
		{
#if DEBUG
			if (eventType <= TraceEventType.Warning)
			{
				Console.WriteLine(String.Concat(GetType().GetReadableFullName(), ": ", eventType));
				Console.WriteLine(message);
				Console.WriteLine(">> Creation stack... ");
				foreach (var frame in CreationStack)
				{
					var method = frame.GetMethod();
					Console.WriteLine(String.Concat("\t >> ", method.DeclaringType.GetReadableSimpleName(), ".", method.Name));
				}
				Console.WriteLine(">> Disposal stack... ");
				foreach (var frame in new System.Diagnostics.StackTrace().GetFrames())
				{	
					var method = frame.GetMethod();
					Console.WriteLine(String.Concat("\t >> ", method.DeclaringType.GetReadableSimpleName(), ".", method.Name));
				}
			}
#endif																																
		}

		/// <summary>
		/// Performs the dispose logic.
		/// </summary>
		/// <param name="disposing">Whether the object is disposing (IDisposable.Dispose method was called).</param>
		/// <returns>Implementers should return true if the disposal was successful; otherwise false.</returns>
		protected abstract bool PerformDispose(bool disposing);
	}


}
