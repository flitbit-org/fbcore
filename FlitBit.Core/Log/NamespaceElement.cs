#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Configuration;
using System.Diagnostics;
using FlitBit.Core.Configuration;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Configuration element collection for specializing logging for a namespaces.
	/// </summary>
	public class NamespaceElementCollection : AbstractConfigurationElementCollection<NamespaceElement, string>
	{
		/// <summary>
		///   Gets the element's key
		/// </summary>
		/// <param name="element">the element</param>
		/// <returns>the key</returns>
		protected override string PerformGetElementKey(NamespaceElement element)
		{
			return element.Namespace;
		}
	}

	/// <summary>
	///   Configuration element for specializing logging for a namespace.
	/// </summary>
	public class NamespaceElement : ConfigurationElement
	{
		const string PropertyNameNamespace = "namespace";
		const string PropertyNameTraceThreshold = "traceThreshold";
		const string PropertyNameStackTraceThreshold = "stackTraceThreshold";
		const string PropertyNameWriterName = "writerName";
		const string PropertyNameWriterType = "writer";
		Type _writerType;

		/// <summary>
		///   The namespace to which the configuration element applies.
		/// </summary>
		[ConfigurationProperty(PropertyNameNamespace
			, IsKey = true
			, IsRequired = true)]
		public string Namespace { get { return (string) this[PropertyNameNamespace]; } set { this[PropertyNameNamespace] = value; } }

		/// <summary>
		///   The source levels.
		/// </summary>
		/// <seealso cref="System.Diagnostics.SourceLevels" />
		[ConfigurationProperty(PropertyNameTraceThreshold)]
    public string TraceThreshold { get { return (string)this[PropertyNameTraceThreshold]; } set { this[PropertyNameTraceThreshold] = value; } }

		/// <summary>
		///   The stack trace threshold
		/// </summary>
		/// <seealso cref="System.Diagnostics.TraceEventType" />
		[ConfigurationProperty(PropertyNameStackTraceThreshold)]
		public string StackTraceThreshold { get { return (string) this[PropertyNameStackTraceThreshold]; } set { this[PropertyNameStackTraceThreshold] = value; } }

		/// <summary>
		///   The name of the sink (in the parent object's 'sinks' collection)
		/// </summary>
		[ConfigurationProperty(PropertyNameWriterName)]
		public string WriterName { get { return (string) this[PropertyNameWriterName]; } set { this[PropertyNameWriterName] = value; } }

		/// <summary>
		///   The type of sink to construct.
		/// </summary>
		[ConfigurationProperty(PropertyNameWriterType)]
		public string WriterTypeName { get { return (string) this[PropertyNameWriterType]; } set { this[PropertyNameWriterType] = value; } }

		internal Type ResolvedWriterType
		{
			get
			{
				return Util.NonBlockingLazyInitializeVolatile(ref this._writerType, () =>
				{
					var name = this.WriterTypeName;
					return (!String.IsNullOrEmpty(name)) ? Type.GetType(name) : null;
				});
			}
		}

	  /// <summary>
	  /// The trace threshold as a value.
	  /// </summary>
	  public TraceEventType TraceThresholdValue
	  {
	    get
	    {
	      TraceEventType res;
	      return Enum.TryParse(TraceThreshold, out res) ? res : default(TraceEventType);
	    }
	  }

	  /// <summary>
	  /// the stack trace threshold as a value.
	  /// </summary>
    public TraceEventType StackTraceThresholdValue
    {
      get
      {
        TraceEventType res;
        return Enum.TryParse(StackTraceThreshold, out res) ? res : default(TraceEventType);
      }
    }
	}
}