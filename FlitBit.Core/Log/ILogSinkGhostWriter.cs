#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Interface for ghost writers. Writing log events are delegated to ghost writers.
	/// </summary>
	public interface ILogSinkGhostWriter
	{
		/// <summary>
		///   Delegates writing an event.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="evt"></param>
		void GhostWrite(LogEventWriter writer, LogEvent evt);
	}
}