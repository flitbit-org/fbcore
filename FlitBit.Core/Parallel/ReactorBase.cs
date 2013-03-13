using System;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   Base class for Reactor&lt;TItem>
	/// </summary>
	public class ReactorBase
	{
		/// <summary>
		///   The default options used by reactors when none are given to the constructor.
		/// </summary>
		public static readonly ReactorOptions DefaultOptions = new ReactorOptions(
			ReactorOptions.DefaultMaxDegreeOfParallelism, false, 0, ReactorOptions.DefaultMaxParallelDepth, 5);

		/// <summary>
		///   Indicates whether the foreground thread has been barrowed by the reactor.
		/// </summary>
		[ThreadStatic]
		protected static bool IsForegroundThreadBorrowed;
	}
}