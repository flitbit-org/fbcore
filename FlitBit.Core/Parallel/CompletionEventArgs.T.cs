using System;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Completion event arguments.
	/// </summary>
	public sealed class CompletionEventArgs<T> : EventArgs
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="completion">the completion</param>
		public CompletionEventArgs(Completion<T> completion)
		{
			this.Completion = completion;
		}
		/// <summary>
		/// Gets the completion upon which the event fired.
		/// </summary>
		public Completion<T> Completion { get; private set; }
	}
}
