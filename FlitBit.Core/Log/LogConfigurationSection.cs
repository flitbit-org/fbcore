#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Configuration;
using System.Diagnostics;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Configuration section for log settings.
	/// </summary>
	public sealed class LogConfigurationSection : ConfigurationSection
	{
		/// <summary>
		///   Default parallel disptach threshold
		/// </summary>
		public const int CDefaultParallelDispatchThreshold = 10000;

		/// <summary>
		///   Default trace threshold.
		/// </summary>
    public const TraceEventType CDefaultTraceThreshold = TraceEventType.Warning;

		/// <summary>
		///   Default stack trace threshold
		/// </summary>
		public const TraceEventType CDefaultStackTraceThreshold = TraceEventType.Warning;

		/// <summary>
		///   Property name for defaultSourceLevel.
		/// </summary>
    public const string PropertyNameDefaultTraceThreshold = "defaultTraceThreshold";

		/// <summary>
		///   Property name for defaultStackTraceThreshold.
		/// </summary>
		public const string PropertyNameDefaultStackTraceThreshold = "defaultStackTraceThreshold";

		/// <summary>
		///   Property name for defaultWriter.
		/// </summary>
		public const string PropertyNameDefaultWriterType = "defaultWriterType";

		/// <summary>
		///   Property name for namespaces
		/// </summary>
		public const string PropertyNameNamespaces = "namespaces";

		/// <summary>
		///   Property name for parallelDispatchThreshold.
		/// </summary>
		public const string PropertyNameParallelDispatchThreshold = "parallelDispatchThreshold";

		/// <summary>
		///   Configuration section name for trace settings.
		/// </summary>
		public static readonly string SectionName = "flitbit.log";

		static LogEventWriter __defaultSink;
		Type _writerType;

		/// <summary>
		///   The default source levels.
		/// </summary>
		/// <seealso cref="System.Diagnostics.SourceLevels" />
		[ConfigurationProperty(PropertyNameDefaultTraceThreshold, DefaultValue = CDefaultTraceThreshold)]
    public TraceEventType DefaultTraceThreshold { get { return (TraceEventType)this[PropertyNameDefaultTraceThreshold]; } set { this[PropertyNameDefaultTraceThreshold] = value; } }

		/// <summary>
		///   The default stack trace threshold
		/// </summary>
		/// <seealso cref="System.Diagnostics.TraceEventType" />
		[ConfigurationProperty(PropertyNameDefaultStackTraceThreshold, DefaultValue = CDefaultStackTraceThreshold)]
		public TraceEventType DefaultStackTraceThreshold { get { return (TraceEventType) this[PropertyNameDefaultStackTraceThreshold]; } set { this[PropertyNameDefaultStackTraceThreshold] = value; } }

		/// <summary>
		///   The default LogEventWriter
		/// </summary>
		[ConfigurationProperty(PropertyNameDefaultWriterType)]
		public string DefaultWriterType { get { return (string) this[PropertyNameDefaultWriterType]; } set { this[PropertyNameDefaultWriterType] = value; } }

		/// <summary>
		///   Gets the confgured namespace elements.
		/// </summary>
		[ConfigurationProperty(PropertyNameNamespaces, IsDefaultCollection = true)]
		public NamespaceElementCollection Namespaces { get { return (NamespaceElementCollection) this[PropertyNameNamespaces]; } }

		/// <summary>
		///   The log sink manager's parallel dispatch threshold.
		/// </summary>
		[ConfigurationProperty(PropertyNameParallelDispatchThreshold, DefaultValue = CDefaultParallelDispatchThreshold)]
		public int ParallelDispatchThreshold { get { return (int) this[PropertyNameParallelDispatchThreshold]; } set { this[PropertyNameParallelDispatchThreshold] = value; } }

		internal LogEventWriter ResolvedDefaultLogWriter
		{
			get
			{
				return Util.NonBlockingLazyInitializeVolatile(ref __defaultSink, () =>
				{
					var writer = (LogEventWriter) Activator.CreateInstance(this.ResolvedDefaultWriterType);
					writer.Initialize("default");
					return writer;
				});
			}
		}

		internal Type ResolvedDefaultWriterType
		{
			get
			{
				return Util.NonBlockingLazyInitializeVolatile(ref this._writerType, () =>
				{
					var name = this.DefaultWriterType;
					return (!String.IsNullOrEmpty(name)) ? Type.GetType(name) : typeof(TraceLogEventWriter);
				});
			}
		}

		/// <summary>
		///   Gets the current configuration section.
		/// </summary>
		public static LogConfigurationSection Current
		{
			get
			{
				var config = ConfigurationManager.GetSection(
																										 SectionName) as LogConfigurationSection;
				return config ?? new LogConfigurationSection();
			}
		}
	}
}