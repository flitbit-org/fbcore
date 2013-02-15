using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Completion event arguments.
	/// </summary>
	public sealed class CompletionEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="completion">the completion</param>
		public CompletionEventArgs(Completion completion)
		{
			this.Completion = completion;
		}
		/// <summary>
		/// The completion upon which the event fired.
		/// </summary>
		public Completion Completion { get; private set; }
	}

}
