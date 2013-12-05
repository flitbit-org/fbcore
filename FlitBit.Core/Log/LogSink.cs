#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Default implementaton of the LogSink interface.
	/// </summary>
	public sealed class LogSink : ILogSinkManagement
	{
		/// <summary>
		///   Default value used for ApplicationSpecificLogEventKind when none is given.
		/// </summary>
		public static readonly int DefaultApplicationSpecificLogEventKind = 0;

		/// <summary>
		///   Default value used for ApplicationSpecificLogEventName when none is given.
		/// </summary>
		public static readonly string DefaultApplicationSpecificLogEventName = String.Empty;

		readonly ILogSinkGhostWriter _ghostWriter;
		readonly string _name;
		ILogSink _next;
		TraceEventType _stackTraceThreshold;
		TraceEventType _traceThreshold;
		LogEventWriter _writer;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="name">the log sink's name</param>
		/// <param name="evtType">a source level</param>
		/// <param name="stackTraceThreshold">the stack threshold</param>
		/// <param name="writer">an event writer</param>
		/// <param name="next">the next sink in the chain or null</param>
		/// <param name="ghostWriter"></param>
    public LogSink(ILogSinkGhostWriter ghostWriter, string name, TraceEventType evtType, TraceEventType stackTraceThreshold,
			LogEventWriter writer, ILogSink next)
		{
			Contract.Requires<ArgumentNullException>(ghostWriter != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(writer != null);

			this._ghostWriter = ghostWriter;
			this._name = name;
		  this._traceThreshold = evtType;
			this._stackTraceThreshold = stackTraceThreshold;
			this._next = next;
			this._writer = writer;
		}

	  /// <summary>
	  ///   The sink's trace event threshold. Determines what actually gets logged.
	  /// </summary>
	  public TraceEventType TraceThreshold
		{
			get
			{
				Thread.MemoryBarrier();
				var value = this._traceThreshold;
				Thread.MemoryBarrier();
				return value;
			}
		}

		[ContractInvariantMethod]
		void InvariantContracts()
		{
			Contract.Invariant(this._writer != null);
		}

		#region ILogSinkManagement Members

		/// <summary>
		///   The sink's name.
		/// </summary>
		public string Name { get { return this._name; } }

		/// <summary>
		///   The next sink in the chain.
		/// </summary>
		public ILogSink NextSink
		{
			get
			{
				Thread.MemoryBarrier();
				var value = this._next;
				Thread.MemoryBarrier();
				return value;
			}
		}

		/// <summary>
		///   The sink's stack trace threshold.
		/// </summary>
		public TraceEventType StackTraceThreshold
		{
			get
			{
				Thread.MemoryBarrier();
				var value = this._stackTraceThreshold;
				Thread.MemoryBarrier();
				return value;
			}
		}

		/// <summary>
		///   The sink's event writer.
		/// </summary>
		public LogEventWriter Writer
		{
			get
			{
				Thread.MemoryBarrier();
				var value = this._writer;
				Thread.MemoryBarrier();
				return value;
			}
		}

	  /// <summary>
	  ///   Indicates whether the log sink is forwarding messages
	  ///   at the source level given.
	  /// </summary>
	  /// <param name="evtType"></param>
	  /// <returns>true if forwarding; otherwise false</returns>
	  public bool IsLogging(TraceEventType evtType)
	  {
	    return evtType <= TraceThreshold 
        || (NextSink != null && NextSink.IsLogging(evtType));
	  }

    void ILogSinkManagement.Reconfigure(TraceEventType evtType, TraceEventType stackTraceThreshold, LogEventWriter writer,
			ILogSink next)
		{
			Contract.Assert(writer != null);

      this._traceThreshold = evtType;
			this._stackTraceThreshold = stackTraceThreshold;
			this._next = next;
			this._writer = writer;
		}

		/// <summary>
		///   Notifies the sink that a critical event occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Critical(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Critical(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a error event occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Error(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Error(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a informational event occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Information(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Information(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a resume activity occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Resume(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Resume(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a start activity occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Start(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Start(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a stop activity occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Stop(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Stop(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a suspend activity occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Suspend(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Suspend(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a transfer activity occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Transfer(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Transfer(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a warning event occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Warning(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Warning(evt);
				}
			}
		}

		/// <summary>
		///   Notifies the sink that a verbose event occurred.
		/// </summary>
		/// <param name="evt">event details</param>
		public void Verbose(LogEvent evt)
		{
			if (evt != null)
			{
				if (evt.EventType <= this.TraceThreshold)
				{
					this._ghostWriter.GhostWrite(this.Writer, evt);
				}
				var next = this.NextSink;
				if (next != null)
				{
					next.Verbose(evt);
				}
			}
		}

		#endregion
	}
}