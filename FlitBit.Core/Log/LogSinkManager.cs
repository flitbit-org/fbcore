#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace FlitBit.Core.Log
{
  /// <summary>
  /// Default log sink manager.
  /// </summary>
  public sealed class LogSinkManager : ILogSinkManager, ILogSinkGhostWriter
  {
    static LogSinkManager __singleton;
    readonly ConcurrentDictionary<string, ILogSink> _logSinks = new ConcurrentDictionary<string, ILogSink>();
    readonly ActionBlock<Tuple<LogEventWriter, LogEvent>> _reactor;
    ILogSink _default;

    internal LogSinkManager()
    {
      this._reactor =
        new ActionBlock<Tuple<LogEventWriter, LogEvent>>(new Action<Tuple<LogEventWriter, LogEvent>>(Bg_WriteLogEvent));
    }

    void Bg_WriteLogEvent(Tuple<LogEventWriter, LogEvent> rec)
    {
      rec.Item1.WriteLogEvent(rec.Item2);
    }

    ILogSink GetFallbackLogSink(string key, string namesp)
    {
      Contract.Requires<ArgumentNullException>(key != null);
      Contract.Requires<ArgumentNullException>(namesp != null);
      Contract.Requires<ArgumentException>(namesp.Length >= 0);

      var config = LogConfigurationSection.Current;

      var rlock = key.InternIt();
      lock (rlock)
      {
        ILogSink result = null;
        var namespaces = new Stack<string>();
        try
        {
          while (SliceNamespace(ref namesp))
          {
            namespaces.Push(namesp);
            Monitor.Enter(namesp.InternIt());
            if (this._logSinks.TryGetValue(namesp, out result))
            {
              break;
            }
            // attempt to load from config...
            var c = config.Namespaces[namesp];
            if (c != null)
            {
              var level = (String.IsNullOrEmpty(c.TraceThreshold))
                            ? config.DefaultTraceThreshold
                            : c.TraceThresholdValue;
              var thresh = (String.IsNullOrEmpty(c.StackTraceThreshold))
                             ? config.DefaultStackTraceThreshold
                             : c.StackTraceThresholdValue;
              if (!String.IsNullOrEmpty(c.WriterName))
              {
                // TODO: resolve log writer by name
              }
              else if (!String.IsNullOrEmpty(c.WriterTypeName))
              {
                var writer = (LogEventWriter)Activator.CreateInstance(c.ResolvedWriterType);
                writer.Initialize(namesp);
                result = new LogSink(this, namesp, level, thresh, writer, null);
              }
              else
              {
                var writer = (LogEventWriter)Activator.CreateInstance(config.ResolvedDefaultWriterType);
                writer.Initialize(namesp);
                result = new LogSink(this, namesp, level, thresh, writer, null);
              }

              break;
            }
          }

          if (result == null)
          {
            var def = this.DefaultLogSink;
            result = new LogSink(this, namesp, def.TraceThreshold, def.StackTraceThreshold, LogEventWriter.NullWriter,
              def);
          }
          while (namespaces.Count > 0)
          {
            var k = namespaces.Pop();
            this._logSinks.TryAdd(k, result);
            Monitor.Exit(k.InternIt());
          }
        }
        finally
        {
          while (namespaces.Count > 0)
          {
            Monitor.Exit(namespaces.Pop()
                                   .InternIt());
          }
        }

        this._logSinks.TryAdd(key, result);
        return result;
      }
    }

    static bool SliceNamespace(ref string namesp)
    {
      Contract.Requires<ArgumentNullException>(namesp != null);
      var sliceAt = namesp.LastIndexOf('.');
      if (sliceAt >= 0)
      {
        // namespaces are interned to make the locking strategy work
        // across time/threads
        namesp = namesp.Substring(0, sliceAt)
                       .InternIt();
      }
      return sliceAt >= 0;
    }

    #region ILogSinkGhostWriter Members

    /// <summary>
    ///   Delegates writing an event.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="evt"></param>
    public void GhostWrite(LogEventWriter writer, LogEvent evt)
    {
      var scheduledWriter = writer ?? DefaultLogSink.Writer;
      if (!ReferenceEquals(LogEventWriter.NullWriter, scheduledWriter))
      {
        this._reactor.Post(Tuple.Create(scheduledWriter, evt));
      }
    }

    #endregion

    #region ILogSinkManager Members

    /// <summary>
    ///   Gets the default log sink.
    /// </summary>
    public ILogSink DefaultLogSink
    {
      get
      {
        return Util.NonBlockingLazyInitializeVolatile(ref this._default, () =>
        {
          var config = LogConfigurationSection.Current;
          var writer = config.ResolvedDefaultLogWriter;
          return new LogSink(this
            , "default"
            , config.DefaultTraceThreshold
            , config.DefaultStackTraceThreshold
            , writer
            , null);
        });
      }
    }

    /// <summary>
    ///   Gets the currently configured log sink for the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ILogSink GetLogSinkForType(Type type)
    {
      Contract.Assert(type != null);

      var key = type.GetLogSourceName();
      ILogSink result = null;

      while (result == null)
      {
        if (this._logSinks.TryGetValue(key, out result))
        {
          return result;
        }
        result = GetFallbackLogSink(key, type.GetReadableFullName());
      }

      return result;
    }

    #endregion

    /// <summary>
    /// Gets the single instance.
    /// </summary>
    public static LogSinkManager Singleton
    {
      get { return Util.NonBlockingLazyInitializeVolatile(ref __singleton, () => new LogSinkManager()); }
    }
  }
}