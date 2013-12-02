using System;
using System.Diagnostics;
using FlitBit.Core.Log;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Base class for Reactor&lt;TItem>
  /// </summary>
  public class ReactorBase
  {
    static readonly ILogSink BaseLogSink = typeof(ReactorBase).GetLogSink();

    /// <summary>
    ///   The default options used by reactors when none are given to the constructor.
    /// </summary>
    public static readonly ReactorOptions DefaultOptions = new ReactorOptions(
      ReactorOptions.DefaultMaxDegreeOfParallelism, false, 0, ReactorOptions.DefaultMaxParallelDepth, 5, false);

    /// <summary>
    ///   Indicates whether the foreground thread has been barrowed by the reactor.
    /// </summary>
    [ThreadStatic]
    protected static bool IsForegroundThreadBorrowed;

    /// <summary>
    /// Gets the class' log sink.
    /// </summary>
    protected virtual ILogSink LogSink { get { return BaseLogSink; } }

    /// <summary>
    /// Fires any uncaught exception handlers.
    /// </summary>
    /// <param name="err">the exception</param>
    /// <returns><em>true</em> if the event handlers indicate the exception should be rethrown; otherwise <em>false</em>.</returns>
    protected bool OnUncaughtException(Exception err)
    {
      if (_uncaughtException == null)
      {
        return false;
      }

      var args = new ReactorExceptionArgs(err);
      _uncaughtException(this, args);
      return args.Rethrow;
    }

    /// <summary>
    ///   Event fired when uncaught exceptions are encountered by the reactor.
    /// </summary>
    public event EventHandler<ReactorExceptionArgs> UncaughtException
    {
      add { _uncaughtException += value; }
      remove { _uncaughtException -= value; }
    }

    event EventHandler<ReactorExceptionArgs> _uncaughtException;
  }
}