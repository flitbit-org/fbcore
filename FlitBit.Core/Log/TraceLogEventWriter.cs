#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Echos log events to a System.Diagnostics trace source.
	/// </summary>
	public class TraceLogEventWriter : LogEventWriter
	{
		TraceSource _source;

		/// <summary>
		///   Initializes the trace source.
		/// </summary>
		/// <param name="sourceName">the trace source's name</param>
		public override void Initialize(string sourceName)
		{
			Contract.Assert(sourceName != null);

			this._source = new TraceSource(sourceName);
		}

		/// <summary>
		///   Writes the log event to a trace source.
		/// </summary>
		/// <param name="evt">the log event</param>
		public override void WriteLogEvent(LogEvent evt)
		{
			Contract.Assert(this._source != null);

			this._source.TraceData(evt.EventType, evt.Kind, evt.ToString());
		}
	}
}