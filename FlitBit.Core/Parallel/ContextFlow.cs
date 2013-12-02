using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
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
		///   Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		public static void Parallel(Action action)
		{
			Contract.Requires<ArgumentNullException>(action != null);

			var ambient = ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (EnsureAmbient((ContextFlow)state))
					{
						try
						{
							action();
						}
						catch (Exception e)
						{
							NotifyUncaughtException(action.Target, e);
						}
					}
				}, ambient);
		}

		/// <summary>
		///   Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		/// <param name="continuation">a continuation called upon completion.</param>
		public static void Parallel(Action action, Continuation continuation)
		{
			Contract.Requires<ArgumentNullException>(action != null);
			Contract.Requires<ArgumentNullException>(continuation != null);

			var ambient = ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (EnsureAmbient((ContextFlow)state))
					{
						try
						{
							try
							{
								action();
							}
							catch (Exception e)
							{
								continuation(e);
								return;
							}
							continuation(null);
						}
						catch (Exception e)
						{
							NotifyUncaughtException(continuation.Target, e);
						}
					}
				}, ambient);
		}

		/// <summary>
		///   Performs the function in parallel.
		/// </summary>
		/// <typeparam name="TResult">result type R</typeparam>
		/// <param name="fun">an action</param>
		/// <param name="continuation">a continuation called upon completion.</param>
		public static void Parallel<TResult>(Func<TResult> fun, Continuation<TResult> continuation)
		{
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Requires<ArgumentNullException>(continuation != null);

			var ambient = ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (EnsureAmbient((ContextFlow)state))
					{
						try
						{
							continuation(null, fun());
						}
						catch (Exception e)
						{
							try
							{
								continuation(e, default(TResult));
							}
							catch (Exception ee)
							{
								NotifyUncaughtException(continuation.Target, ee);
							}
						}
					}
				}, ambient);
		}

		/// <summary>
		///   Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		/// <returns>a completion</returns>
		public static Completion ParallelWithCompletion(Action action)
		{
			Contract.Requires<ArgumentNullException>(action != null);
			Contract.Ensures(Contract.Result<Completion>() != null);

			var waitable = new Completion(action.Target);
			var ambient = ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (EnsureAmbient((ContextFlow)state))
					{
						try
						{
							action();
							waitable.MarkCompleted();
						}
						catch (Exception e)
						{
							waitable.MarkFaulted(e);
						}
					}
				}, ambient);
			return waitable;
		}

		/// <summary>
		///   Performs the function in parallel.
		/// </summary>
		/// <typeparam name="TResult">result type R</typeparam>
		/// <param name="fun">an action</param>
		/// <returns>a completion where the results will be returned upon completion</returns>
		public static Completion<TResult> ParallelWithCompletion<TResult>(Func<TResult> fun)
		{
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Ensures(Contract.Result<Completion<TResult>>() != null);

			var waitable = new Completion<TResult>(fun.Target);
			var ambient = ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (EnsureAmbient((ContextFlow)state))
					{
						try
						{
							waitable.MarkCompleted(fun());
						}
						catch (Exception e)
						{
							waitable.MarkFaulted(e);
						}
					}
				}, ambient);
			return waitable;
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
	  public static Action Capture(Action task)
	  {
		  var ctx = ForkAmbient();
			return () =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task();
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
	  }

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg> Capture<TArg>(Action<TArg> task)
		{
			var ctx = ForkAmbient();
			return a =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1> Capture<TArg, TArg1>(
			Action<TArg, TArg1> task)
		{
			var ctx = ForkAmbient();
			return (a, a1) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2> Capture<TArg, TArg1, TArg2>(
			Action<TArg, TArg1, TArg2> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3> Capture<TArg, TArg1, TArg2, TArg3>(
			Action<TArg, TArg1, TArg2, TArg3> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4> Capture<TArg, TArg1, TArg2, TArg3, TArg4>(
			Action<TArg, TArg1, TArg2, TArg3, TArg4> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5>(
			Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4, a5);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
			Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4, a5, a6);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					} 
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
			Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4, a5, a6, a7);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
			Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7, a8) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4, a5, a6, a7, a8);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(Action<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
			{
				using (EnsureAmbient(ctx))
				{
					try
					{
						task(a, a1, a2, a3, a4, a5, a6, a7, a8, a9);
					}
					catch (Exception e)
					{
						NotifyUncaughtException(task.Target, e);
					}
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TResult> Capture<TResult>(
			Func<TResult> task)
		{
			var ctx = ForkAmbient();
			return () =>
			{
				using (EnsureAmbient(ctx))
				{
					return task();
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TResult> Capture<TArg, TResult>(
			Func<TArg, TResult> task)
		{
			var ctx = ForkAmbient();
			return a =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TResult> Capture<TArg, TArg1, TResult>(
			Func<TArg, TArg1, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TResult> Capture<TArg, TArg1, TArg2, TResult>(
			Func<TArg, TArg1, TArg2, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TResult> Capture<TArg, TArg1, TArg2, TArg3, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
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
					return task(a, a1, a2, a3, a4);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3, a4, a5);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3, a4, a5, a6);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3, a4, a5, a6, a7);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7, a8) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3, a4, a5, a6, a7, a8);
				}
			};
		}

		/// <summary>
		/// Creates an action that restores the ambient context around the specified task.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Capture<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(
			Func<TArg, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> task)
		{
			var ctx = ForkAmbient();
			return (a, a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
			{
				using (EnsureAmbient(ctx))
				{
					return task(a, a1, a2, a3, a4, a5, a6, a7, a8, a9);
				}
			};
		}

	  ContextFlow()
		  : this(false)
	  {}

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
        const string message = "Context disposed out of order. Thar be shenanigans in the disposery!";
        try
        {
	        if (LogSink.IsLogging(SourceLevels.Error))
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
			        LogSink.Error(() =>
				        String.Concat(message, builder));
		        }
		        else
		        {
			        LogSink.Error(message);
		        }
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
        foreach(var tpl in _contexts)
        {
	        tpl.Item1.Detach(this, tpl.Item2);
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
	      ambient.Attach();
        return ambient;
      }
	    return null;
    }

		private void Attach()
		{
			foreach (var tpl in _contexts)
			{
				tpl.Item1.Attach(this, tpl.Item2);
			}
		}

	  /// <summary>
	  ///   Prepares a copy of the context for use in thread-pool and background threads.
	  /// </summary>
	  /// <returns>an instance suitable for use as a background thread's ambient context</returns>
	  public static ContextFlow ForkAmbient()
	  {
			var contexts = (from provider in Providers let key = provider.Capture() where key != null select Tuple.Create(provider, key)).ToList();
			return new ContextFlow(contexts);
	  }

	  internal static class Ambient
    {
	    const string CallContextKey = "FlitBitAmbientContextFlowKey";
      [ThreadStatic] static Stack<ContextFlow> __stack;

      internal static ContextFlow Push(ContextFlow ambient)
      {
        // .net 4.0 TPL doesn't flow the call context well so we'll keep track ourselves...
        var stack = __stack; //(Stack<ContextFlow>) CallContext.LogicalGetData(CallContextKey);
				if (stack == null)
        {
					__stack = stack = new Stack<ContextFlow>();
	        //CallContext.LogicalSetData(CallContextKey, stack);
        }
				stack.Push(ambient);
        return ambient;
      }

      internal static bool TryPeek(out ContextFlow ambient)
      {
        var stack = __stack; // (Stack<ContextFlow>) CallContext.LogicalGetData(CallContextKey);
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
        var stack = __stack; // (Stack<ContextFlow>)CallContext.LogicalGetData(CallContextKey);
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
    }

	  /// <summary>
	  /// Gets the current context flow if one exists.
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

	  private static readonly ConcurrentBag<IContextFlowProvider> Providers = new ConcurrentBag<IContextFlowProvider>();
		readonly List<Tuple<IContextFlowProvider, object>> _contexts; 
		internal static void RegisterProvider(IContextFlowProvider provider)
		{
			Providers.Add(provider);
		}
	}
}