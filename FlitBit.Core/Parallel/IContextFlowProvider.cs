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
		/// <param name="captureKey">the capture key</param>
		void Attach(ContextFlow context, object captureKey);

		/// <summary>
		/// Callback invoked by the context flow engine to detach the provider's context from the current thread.
		/// </summary>
		/// <param name="context">the local thread's context</param>
		/// <param name="captureKey">the capture key</param>
		void Detach(ContextFlow context, object captureKey);
	}
}
