#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core.Process;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Contains information related to log events.
	/// </summary>
	[Serializable]
	public abstract class LogEvent
	{
		static readonly int CHashCodeSeed = typeof(LogEvent).AssemblyQualifiedName.GetHashCode();
		static readonly string CacheComponentName;

		static readonly string CacheEnvironmentName;
		static readonly string CacheMachineName;
		static readonly int CacheProcessID;
		static readonly string CacheProcessName;
		static int __processSequence;

		readonly int _sequence;
		string _cachedStringRepresentation;

		static LogEvent()
		{
			var processID = ProcessIdentity.MakeProcessIdentity();
			CacheEnvironmentName = processID.Environment;
			CacheMachineName = processID.MachineName;
			CacheComponentName = processID.Component;
			CacheProcessName = processID.ProcessName;
			CacheProcessID = processID.ProcessID;
		}

		/// <summary>
		///   Creates a new log event.
		/// </summary>
		/// <param name="source">the source of the event</param>
		/// <param name="eventType">the event type</param>
		/// <param name="appKind">an application specific event kind</param>
		/// <param name="appKindName">an application specific event name</param>
		/// <param name="stackTrace">a stack trace associated with the event</param>
		protected LogEvent(String source, TraceEventType eventType, int appKind, String appKindName, StackTrace stackTrace)
		{
			Contract.Requires<ArgumentNullException>(source != null);

			this._sequence = Interlocked.Increment(ref __processSequence);
			this.TimestampUTC = DateTime.UtcNow;
			this.ThreadID = Thread.CurrentThread.ManagedThreadId;
			this.SourceName = source;
			this.EventType = eventType;
			this.Kind = appKind;
			this.KindName = appKindName ?? LogSink.DefaultApplicationSpecificLogEventName;
			this.StackTrace = (stackTrace != null)
				? stackTrace.ToString()
				: String.Empty;
		}

		/// <summary>
		///   Gets a message associated with the event.
		/// </summary>
		public abstract string Message { get; }

		/// <summary>
		///   Gets the component name.
		/// </summary>
		/// <remarks>
		///   Component names are established in the configuration
		///   file and are used to differentiate processes from one-another.
		///   Component names are a friendly name for a process and work
		///   well when they identify the process's role such as
		///   'commercial site', 'corporate site' and 'backend'.
		/// </remarks>
		public string ComponentName { get { return CacheComponentName; } }

		/// <summary>
		///   Gets the name of the environment where the event originated.
		/// </summary>
		/// <remarks>
		///   Environment names are established in the configuration
		///   file and are used to differentiate environments where log events
		///   originate. It only becomes relevant if you collect log events
		///   from more than one environment and want to differentiate events
		///   by environment at a later time.
		/// </remarks>
		public string EnvironmentName { get { return CacheEnvironmentName; } }

		/// <summary>
		///   Gets the event type.
		/// </summary>
		public TraceEventType EventType { get; private set; }

		/// <summary>
		///   Gets the event's kind.
		/// </summary>
		/// <remarks>
		///   Event kinds are application specific and should
		///   be used to identify specific events or event kinds. While
		///   the EventType is a standardized 'event type' indicator,
		///   event kinds allow your own identity or severity scheme
		///   over log events.
		/// </remarks>
		public int Kind { get; protected set; }

		/// <summary>
		///   Gets the name of the event's kind.
		/// </summary>
		/// <remarks>
		///   Event kinds are application specific and should
		///   be used to identify specific events or event kinds. While
		///   the EventType is a standardized 'event type' indicator,
		///   event kinds allow your own identity or severity scheme
		///   over log events.
		/// </remarks>
		public string KindName { get; protected set; }

		/// <summary>
		///   Gets the machine's name (NetBIOS) where the event originated.
		/// </summary>
		public string MachineName { get { return CacheMachineName; } }

		/// <summary>
		///   Gets the process ID where the event originated.
		/// </summary>
		public int ProcessID { get { return CacheProcessID; } }

		/// <summary>
		///   Gets the process name where the event originated.
		/// </summary>
		public string ProcessName { get { return CacheProcessName; } }

		/// <summary>
		///   Sequence suitable for ordering the log events in the order
		///   in which they were produced within the source process.
		/// </summary>
		public int ProcessSequence { get { return this._sequence; } }

		/// <summary>
		///   Gets the name of the log source that originated the event.
		/// </summary>
		public string SourceName { get; private set; }

		/// <summary>
		///   Gets the stack trace associated with the log message.
		/// </summary>
		public string StackTrace { get; protected set; }

		/// <summary>
		///   Gets the thread ID where the event originated.
		/// </summary>
		public int ThreadID { get; private set; }

		/// <summary>
		///   Gets a timestamp indicating when the event originated.
		/// </summary>
		public DateTime TimestampUTC { get; private set; }

		/// <summary>
		///   Determines if the log event is equal to another object.
		/// </summary>
		/// <param name="obj">the other object</param>
		/// <returns>true if equal; otherwise false</returns>
		public override bool Equals(object obj)
		{
			return (obj is LogEvent) && Equals((LogEvent) obj);
		}

		/// <summary>
		///   Gets the event's hashcode.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			const int prime = Constants.NotSoRandomPrime;
			var code = CHashCodeSeed * prime;
			code ^= this.ProcessID * prime;
			code ^= this.ProcessSequence * prime;
			code ^= this.ThreadID * prime;
			code ^= this.TimestampUTC.GetHashCode() * prime;
			code ^= this.KindName.GetHashCode() * prime;
			code ^= this.EnvironmentName.GetHashCode() * prime;
			code ^= this.ComponentName.GetHashCode() * prime;
			return code;
		}

		/// <summary>
		///   Gets the string representation of the log event.
		/// </summary>
		/// <returns>string representation</returns>
		public override string ToString()
		{
			return Util.NonBlockingLazyInitializeVolatile(ref this._cachedStringRepresentation, () => this.ToJson());
		}

		/// <summary>
		///   Determines if the log event is equal to another.
		/// </summary>
		/// <param name="other">the other</param>
		/// <returns>true if equal; otherwise false</returns>
		public bool Equals(LogEvent other)
		{
			return other != null
				&& other.ProcessID == this.ProcessID
				&& other.ProcessSequence == this.ProcessSequence
				&& other.ThreadID == this.ThreadID
				&& other.TimestampUTC == this.TimestampUTC
				&& other.KindName.Equals(this.KindName)
				&& other.EnvironmentName.Equals(this.EnvironmentName)
				&& other.ComponentName.Equals(this.ComponentName);
		}
	}
}