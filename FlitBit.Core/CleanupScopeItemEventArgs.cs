using System;

namespace FlitBit.Core
{
	/// <summary>
	///   Provides information about cleanup scope events.
	/// </summary>
	public sealed class CleanupScopeItemEventArgs : EventArgs
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="item">the item that caused the event</param>
		public CleanupScopeItemEventArgs(object item)
		{
			this.Item = item;
		}

		/// <summary>
		///   The item that caused the event.
		/// </summary>
		public object Item { get; private set; }
	}
}