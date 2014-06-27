#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Interface for classes that provide additional context to an operation's context flow.
	/// </summary>
	public interface IContextFlowProvider
	{
		/// <summary>
		/// The provider's unique context key.
		/// </summary>
		Guid ContextKey { get; }

		/// <summary>
		/// Callback invoked by the context flow engine to capture context when context needs to flow across threads.
		/// </summary>
		/// <returns>an opaque object that the provider can use to correlate the context on another thread</returns>
		object Capture();

	  /// <summary>
	  /// Callback invoked by the context flow engine to attach a previously captured context to the current thread.
	  /// </summary>
	  /// <param name="context">the local thread's context</param>
	  /// <param name="capture">the object captured by context flow when forking a context</param>
	  /// <returns>an opaque handback object passed back to the detach method upon context completion</returns>
	  object Attach(ContextFlow context, object capture);

	  /// <summary>
	  /// Callback invoked by the context flow engine to detach the provider's context from the current thread.
	  /// </summary>
	  /// <param name="context">the local thread's context</param>
	  /// <param name="attachment">the capture key</param>
	  /// <param name="err">an uncaught exception that caused context completion or null if successful</param>
	  void Detach(ContextFlow context, object attachment, Exception err);
	}
}
