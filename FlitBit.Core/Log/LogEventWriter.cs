#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Writes log events onto a more durable store or to another resource.
	/// </summary>
	public abstract class LogEventWriter
	{
		/// <summary>
		///   An event writer stub for use when no configuration exists.
		/// </summary>
		public static readonly LogEventWriter NullWriter = new NullLogEventWriter();

		/// <summary>
		///   Initializes the event writer.
		/// </summary>
		/// <param name="sourceName">the name of the log source that will be using the writer.</param>
		public abstract void Initialize(string sourceName);

		/// <summary>
		///   Writes log events.
		/// </summary>
		/// <param name="evt">the event</param>
		public abstract void WriteLogEvent(LogEvent evt);
	}
}