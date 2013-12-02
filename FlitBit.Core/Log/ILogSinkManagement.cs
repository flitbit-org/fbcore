#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Diagnostics;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Interface allowing log sink management.
	/// </summary>
	public interface ILogSinkManagement : ILogSink
	{
		/// <summary>
		///   Reconfigures a log sink.
		/// </summary>
		/// <param name="level">a source level</param>
		/// <param name="stackTraceThreshold">a stacktrace threshold</param>
		/// <param name="writer">an event writer</param>
		/// <param name="next">the next sink in the chain</param>
		void Reconfigure(SourceLevels level, TraceEventType stackTraceThreshold, LogEventWriter writer, ILogSink next);
	}
}