#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Manages log sinks.
	/// </summary>
	public interface ILogSinkManager
	{
		/// <summary>
		///   Gets the default log sink.
		/// </summary>
		ILogSink DefaultLogSink { get; }

		/// <summary>
		///   Gets the currently configured log sink for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		ILogSink GetLogSinkForType(Type type);
	}
}