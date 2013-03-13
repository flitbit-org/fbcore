#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Text;

namespace FlitBit.Core
{
	/// <summary>
	///   Abstract logic for disposable objects.
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
		///   Creates a new instance.
		/// </summary>
		protected Disposable()
			: this(true) { }
#else
	/// <summary>
	///   Creates a new instance.
	/// </summary>
		protected Disposable() : this(false)
		{																
		}																
#endif

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		protected Disposable(bool captureStack)
		{
			if (captureStack)
			{
				this.CreationStack = new StackTrace().GetFrames();
			}
		}

		/// <summary>
		///   Exposes the call stack at the time of creation if the subtype indicated that it should be captured.
		/// </summary>
		protected StackFrame[] CreationStack { get; private set; }

		/// <summary>
		///   Finalizes the instance.
		/// </summary>
		~Disposable() { this.Dispose(false); }

		/// <summary>
		///   Disposes the instance.
		/// </summary>
		public void Dispose()
		{
			if (Dispose(true))
			{
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		///   Indicates whether the instance has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _disposal.CurrentState == DisposalState.Disposed; }
		}

		bool Dispose(bool disposing)
		{
			while (_disposal.IsLessThan(DisposalState.Disposed))
			{
				if (_disposal.HasState(DisposalState.Disposing))
				{
					if (!_disposal.TrySpinWaitForState(DisposalState.Incomplete, state => state == DisposalState.Disposed))
					{
						try
						{
							if (LogSink.ShouldTrace(this, TraceEventType.Error))
							{
								var cstack = CreationStack;
								if (cstack != null)
								{
									var builder = new StringBuilder(2000);
									builder.Append(Environment.NewLine).Append(">> Creation stack... ");
									foreach (var frame in CreationStack)
									{
										var method = frame.GetMethod();
										builder.Append(Environment.NewLine)
													.Append("\t >> ")
													.Append(method.DeclaringType.GetReadableSimpleName()).Append(".").Append(method.Name);
									}
									builder.Append(Environment.NewLine).Append(">> Disposal stack... ");
									var stackFrames = new StackTrace().GetFrames();
									if (stackFrames != null)
									{
										foreach (var frame in stackFrames)
										{
											var method = frame.GetMethod();
											builder.Append(Environment.NewLine)
														.Append("\t >> ")
														.Append(method.DeclaringType.GetReadableSimpleName()).Append(".").Append(method.Name);
										}
									}

									LogSink.OnTraceEvent(this, TraceEventType.Error, String.Concat("Disposed object disposed again.", builder));
								}
								else
								{
									LogSink.OnTraceEvent(this, TraceEventType.Error, "Disposed object disposed again.");
								}
							}
						}
// ReSharper disable EmptyGeneralCatchClause
						catch
// ReSharper restore EmptyGeneralCatchClause
						{
							/* purposely eating exceptions in finalizer */
						}
						return false;
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
		///   Performs the dispose logic.
		/// </summary>
		/// <param name="disposing">Whether the object is disposing (IDisposable.Dispose method was called).</param>
		/// <returns>Implementers should return true if the disposal was successful; otherwise false.</returns>
		protected abstract bool PerformDispose(bool disposing);
	}
}