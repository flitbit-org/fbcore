﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using FlitBit.Core.Log;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Utility class for managing context flow.
  /// </summary>
  public sealed class ContextFlow : Disposable
  {
    static readonly ILogSink LogSink = typeof(ContextFlow).GetLogSink();

    static EventHandler<UncaughtExceptionArgs> __onUncaughtException;

    /// <summary>
    ///   Event fired when uncaught exceptions are raised by parallel operations.
    /// </summary>
    public static event EventHandler<UncaughtExceptionArgs> OnUncaughtException
    {
      add { __onUncaughtException += value; }
      remove
      {
        if (__onUncaughtException != null)
        {
          // ReSharper disable once DelegateSubtraction
          __onUncaughtException -= value;
        }
      }
    }

    /// <summary>
    ///   Notifies event handlers that an exception has occurred in a paralle operation.
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="e">the exception that was raised.</param>
    public static void NotifyUncaughtException(object sender, Exception e)
    {
      LogSink.Error(String.Concat("An uncaught exception occurred in a background thread.", e.FormatForLogging()));
      if (__onUncaughtException != null)
      {
        __onUncaughtException(sender, new UncaughtExceptionArgs(e));
      }
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action Capture(Action task)
    {
      var ctx = ForkAmbient();
      return () =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task();
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    /// An uncaught exception observed by the context.
    /// </summary>
    public Exception AmbientException { get; private set; }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg> Capture<TArg>(Action<TArg> task)
    {
      var ctx = ForkAmbient();
      return a =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1> Capture<TArg, TArg1>(
      Action<TArg, TArg1> task)
    {
      var ctx = ForkAmbient();
      return (a, a1) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2> Capture<TArg, TArg1, TArg2>(
      Action<TArg, TArg1, TArg2> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3> Capture<TArg, TArg1, TArg2, TArg3>(
      Action<TArg, TArg1, TArg2, TArg3> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4> Capture<TArg, TArg1, TArg2, TArg3, TArg4>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4, a5);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4, a5, a6);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4, a5, a6, a7);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7, a8) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4, a5, a6, a7, a8);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(
      Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            task(a, a1, a2, a3, a4, a5, a6, a7, a8, a9);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TResult> Capture<TResult>(
      Func<TResult> task)
    {
      var ctx = ForkAmbient();
      return () =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task();
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TResult> Capture<TArg, TResult>(
      Func<TArg, TResult> task)
    {
      var ctx = ForkAmbient();
      return a =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TResult> Capture<TArg, TArg1, TResult>(
      Func<TArg, TArg1, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TResult> Capture<TArg, TArg1, TArg2, TResult>(
      Func<TArg, TArg1, TArg2, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TResult> Capture<TArg, TArg1, TArg2, TArg3, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4) =>
      {
        using (EnsureAmbient(ctx))
        {
          using (var inner = EnsureAmbient(ctx))
          {
            try
            {
              return task(a, a1, a2, a3, a4);
            }
            catch (Exception e)
            {
              inner.AmbientException = e;
              NotifyUncaughtException(task.Target, e);
              throw;
            }
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3, a4, a5);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3, a4, a5, a6);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3, a4, a5, a6, a7);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7, a8) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3, a4, a5, a6, a7, a8);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }
      };
    }

    /// <summary>
    ///   Creates an action that restores the ambient context around the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Capture
      <TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(
      Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> task)
    {
      var ctx = ForkAmbient();
      return (a, a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
      {
        using (var inner = EnsureAmbient(ctx))
        {
          try
          {
            return task(a, a1, a2, a3, a4, a5, a6, a7, a8, a9);
          }
          catch (Exception e)
          {
            inner.AmbientException = e;
            NotifyUncaughtException(task.Target, e);
            throw;
          }
        }

      };
    }

    ContextFlow(bool independent)
    {
      if (!independent)
      {
        Ambient.Push(this);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="contexts"></param>
    public ContextFlow(List<Tuple<IContextFlowProvider, object>> contexts)
      : this(true)
    {
      this._contexts = contexts;
    }

    /// <summary>
    ///   Performs the disposal.
    /// </summary>
    /// <param name="disposing">indicates whether the context is disposing</param>
    /// <returns>
    ///   <em>true</em> if disposed as a result of the call; otherwise <em>false</em>
    /// </returns>
    protected override bool PerformDispose(bool disposing)
    {
      if (disposing && !Ambient.TryPop(this))
      {
        const string message = "Unchained context disposed!";
        try
        {
          var cstack = this.CreationStack;
          if (cstack != null)
          {
            var builder = new StringBuilder(2000);
            builder.Append(Environment.NewLine)
                   .Append(">> Creation stack... ");
            foreach (var frame in this.CreationStack)
            {
              var method = frame.GetMethod();
              builder.Append(Environment.NewLine)
                     .Append("\t >> ")
                     .Append(method.DeclaringType.GetReadableSimpleName())
                     .Append(".")
                     .Append(method.Name);
            }
            builder.Append(Environment.NewLine)
                   .Append(">> Disposal stack... ");
            var stackFrames = new StackTrace().GetFrames();
            if (stackFrames != null)
            {
              foreach (var frame in stackFrames)
              {
                var method = frame.GetMethod();
                builder.Append(Environment.NewLine)
                       .Append("\t >> ")
                       .Append(method.DeclaringType.GetReadableSimpleName())
                       .Append(".")
                       .Append(method.Name);
              }
            }
            LogSink.Information(() =>
                                String.Concat(message, builder));
          }
          else
          {
            LogSink.Information(message);
          }
        }
// ReSharper disable once EmptyGeneralCatchClause
        catch
        {
          /* purposely eating exceptions in finalizer */
        }
      }
      if (disposing)
      {
        foreach (var tpl in _attachments)
        {
          tpl.Item1.Detach(this, tpl.Item2, AmbientException);
        }
      }
      return true;
    }

    /// <summary>
    ///   Attaches the given context to the current thread.
    /// </summary>
    /// <param name="ambient">a new ambient context</param>
    /// <returns>the provided context</returns>
    public static ContextFlow EnsureAmbient(ContextFlow ambient)
    {
      if (ambient != null)
      {
        Ambient.Attach(ambient);
        return ambient;
      }
      return null;
    }

    void Attach()
    {
      var attachements = this._contexts.Select(tpl => Tuple.Create(tpl.Item1, tpl.Item1.Attach(this, tpl.Item2))).ToList();
      _attachments = attachements;
    }

    /// <summary>
    ///   Prepares a copy of the context for use in thread-pool and background threads.
    /// </summary>
    /// <returns>an instance suitable for use as a background thread's ambient context</returns>
    public static ContextFlow ForkAmbient()
    {
      var contexts =
        (from provider in Providers let key = provider.Capture() where key != null select Tuple.Create(provider, key))
          .ToList();
      return new ContextFlow(contexts);
    }

    internal static class Ambient
    {
      const string CallContextKey = "FlitBitAmbientContextFlowKey";

      internal static ContextFlow Push(ContextFlow ambient)
      {
        var stack = (Stack<ContextFlow>) CallContext.LogicalGetData(CallContextKey);
        if (stack == null)
        {
          CallContext.LogicalSetData(CallContextKey, stack = new Stack<ContextFlow>());
        }
        stack.Push(ambient);
        return ambient;
      }

      internal static bool TryPeek(out ContextFlow ambient)
      {
        var stack = (Stack<ContextFlow>)CallContext.LogicalGetData(CallContextKey);
        if (stack != null
            && stack.Count > 0)
        {
          ambient = stack.Peek();
          return true;
        }
        ambient = default(ContextFlow);
        return false;
      }

      internal static bool TryPop(ContextFlow comparand)
      {
        var stack = (Stack<ContextFlow>)CallContext.LogicalGetData(CallContextKey);
        if (comparand != null
            && stack != null
            && stack.Count > 0)
        {
          var ambient = stack.Peek();
          if (ReferenceEquals(ambient, comparand))
          {
            stack.Pop();
            return true;
          }
        }
        return false;
      }

      internal static ContextFlow Attach(ContextFlow ambient)
      {
        var stack = (Stack<ContextFlow>)CallContext.LogicalGetData(CallContextKey);
        if (stack == null)
        {
          stack = new Stack<ContextFlow>();
          CallContext.LogicalSetData(CallContextKey, stack);
        }
        if (stack.Count > 0)
        {
          throw new InvalidOperationException(
            "ContextFlow detected a context being attached over the top of an existing context");
        }
        ambient.Attach();
        stack.Push(ambient);
        return ambient;
      }
    }

    /// <summary>
    ///   Gets the current context flow if one exists.
    /// </summary>
    public static ContextFlow Current
    {
      get
      {
        ContextFlow res;
        Ambient.TryPeek(out res);
        return res;
      }
    }

    static readonly ConcurrentBag<IContextFlowProvider> Providers = new ConcurrentBag<IContextFlowProvider>();
    readonly List<Tuple<IContextFlowProvider, object>> _contexts;
    List<Tuple<IContextFlowProvider, object>> _attachments;

    /// <summary>
    ///   Registers a context flow provider.
    /// </summary>
    /// <param name="provider">the provider</param>
    public static void RegisterProvider(IContextFlowProvider provider)
    {
      Providers.Add(provider);
    }
  }
}