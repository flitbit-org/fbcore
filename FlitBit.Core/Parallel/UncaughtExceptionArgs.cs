using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.Core
{
	/// <summary>
	/// Event args for uncaught exceptions.
	/// </summary>
	public class UncaughtExceptionArgs : EventArgs
	{		
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="e"></param>
		public UncaughtExceptionArgs(Exception e)
		{
			this.Error = e;
		}
		/// <summary>
		/// The uncaught exception.
		/// </summary>
		public Exception Error { get; private set; }
	}
}
