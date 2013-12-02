#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Log message for data.
	/// </summary>
	/// <typeparam name="TData">data type TData</typeparam>
	[Serializable]
	public sealed class DataLogEvent<TData> : LazyLogEvent
	{
		/// <summary>
		///   Creates a new log event.
		/// </summary>
		/// <param name="source">the source of the event</param>
		/// <param name="eventType">the event type</param>
		/// <param name="appKind">an application specific event kind</param>
		/// <param name="appKindName">an application specific event name</param>
		/// <param name="data">data to be transformed for data</param>
		/// <param name="stackTrace">a stack trace associated with the event</param>
		public DataLogEvent(String source, TraceEventType eventType, int appKind, string appKindName, TData data,
			StackTrace stackTrace)
			: base(source, eventType, appKind, appKindName, () => LogDataTransform<TData>.Transform(data), stackTrace)
		{
			Contract.Requires<ArgumentNullException>(source != null);
		}
	}

	/// <summary>
	///   Log message for data.
	/// </summary>
	/// <typeparam name="TData">data type TData</typeparam>
	[Serializable]
	public sealed class LazyDataLogEvent<TData> : LazyLogEvent
	{
		/// <summary>
		///   Creates a new log event.
		/// </summary>
		/// <param name="source">the source of the event</param>
		/// <param name="eventType">the event type</param>
		/// <param name="appKind">an application specific event kind</param>
		/// <param name="appKindName">an application specific event name</param>
		/// <param name="data">data to be transformed for data</param>
		/// <param name="stackTrace">a stack trace associated with the event</param>
		public LazyDataLogEvent(String source, TraceEventType eventType, int appKind, string appKindName, Func<TData> data,
			StackTrace stackTrace)
			: base(source, eventType, appKind, appKindName, () => LogDataTransform<TData>.Transform(data()), stackTrace)
		{
			Contract.Requires<ArgumentNullException>(source != null);
		}
	}
}