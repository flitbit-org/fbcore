#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Parallelism utilities.
	/// </summary>
	public static class Go
	{
		/// <summary>
		/// Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		public static void Parallel(Action action)
		{
			Contract.Requires<ArgumentNullException>(action != null);

			Completion waitable = new Completion(action.Target);
			var ambient = ContextFlow.ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				ignored =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
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
				});
		}

		/// <summary>
		/// Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		/// <param name="continuation">a continuation called upon completion.</param>
		public static void Parallel(Action action, Continuation continuation)
		{
			Contract.Requires<ArgumentNullException>(action != null);
			Contract.Requires<ArgumentNullException>(continuation != null);

			Completion waitable = new Completion(action.Target);
			var ambient = ContextFlow.ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				ignored =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
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
				});
		}

		/// <summary>
		/// Performs the function in parallel.
		/// </summary>
		/// <typeparam name="R">result type R</typeparam>
		/// <param name="fun">an action</param>
		/// <param name="continuation">a continuation called upon completion.</param>
		public static void Parallel<R>(Func<R> fun, Continuation<R> continuation)
		{
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Requires<ArgumentNullException>(continuation != null);

			var ambient = ContextFlow.ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				ignored =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
					{
						try
						{
							continuation(null, fun());
						}
						catch (Exception e)
						{
							try
							{
								continuation(e, default(R));
							}
							catch (Exception ee)
							{
								NotifyUncaughtException(continuation.Target, ee);
							}
						}
					}
				});
		}

		/// <summary>
		/// Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		/// <returns>a completion</returns>
		public static Completion ParallelWithCompletion(Action action)
		{
			Contract.Requires<ArgumentNullException>(action != null);
			Contract.Ensures(Contract.Result<Completion>() != null);

			Completion waitable = new Completion(action.Target);
			var ambient = ContextFlow.ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				ignored =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
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
				});
			return waitable;
		}
		
		/// <summary>
		/// Performs the function in parallel.
		/// </summary>
		/// <typeparam name="R">result type R</typeparam>
		/// <param name="fun">an action</param>
		/// <returns>a completion where the results will be returned upon completion</returns>
		public static Completion<R> ParallelWithCompletion<R>(Func<R> fun)
		{
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Ensures(Contract.Result<Completion<R>>() != null);

			Completion<R> waitable = new Completion<R>(fun.Target);
			var ambient = ContextFlow.ForkAmbient();
			ThreadPool.QueueUserWorkItem(
				ignored =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
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
				});
			return waitable;
		}
		
		static EventHandler<UncaughtExceptionArgs> _onUncaughtException;

		/// <summary>
		/// Event fired when uncaught exceptions are raised by parallel operations.
		/// </summary>
		public static event EventHandler<UncaughtExceptionArgs> OnUncaughtException
		{
			add { _onUncaughtException += value; }
			remove { _onUncaughtException -= value; }
		}

		/// <summary>
		/// Notifies event handlers that an exception has occurred in a paralle operation.
		/// </summary>
		/// <param name="sender">the sender</param>
		/// <param name="e">the exception that was raised.</param>
		public static void NotifyUncaughtException(object sender, Exception e)
		{
			if (_onUncaughtException != null)
			{
				_onUncaughtException(sender, new UncaughtExceptionArgs(e));
			}
		}
	}
}
