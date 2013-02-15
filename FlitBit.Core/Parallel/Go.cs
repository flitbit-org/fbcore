#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Parallelism using thread pool.
	/// </summary>
	public static class Go
	{
		/// <summary>
		/// Performs an action in parallel.
		/// </summary>
		/// <param name="action">an action</param>
		/// <returns>a completion</returns>
		public static Completion Parallel(Action action)
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
		public static Completion<R> Parallel<R>(Func<R> fun)
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
		/// Event fired when uncaught exceptions are raised by parallel executions.
		/// </summary>
		public static event EventHandler<UncaughtExceptionArgs> OnUncaughtException
		{
			add { _onUncaughtException += value; }
			remove { _onUncaughtException -= value; }
		}

		internal static void NotifyUncaughtException(object sender, Exception e)
		{
			if (_onUncaughtException != null)
			{
				_onUncaughtException(sender, new UncaughtExceptionArgs(e));
			}
		}
	}
}
