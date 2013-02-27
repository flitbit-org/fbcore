using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Utility class for managing context flow.
	/// </summary>
	public sealed class ContextFlow : Disposable
	{
		internal static class Ambient
		{
			[ThreadStatic]
			static Stack<ContextFlow> __stack;

			internal static bool TryPeek(out ContextFlow ambient)
			{
				if (__stack != null && __stack.Count > 0)
				{	
					ambient = __stack.Peek();
					return true;
				}
				ambient = default(ContextFlow);
				return false;
			}

			internal static bool TryPop(ContextFlow comparand)
			{
				if (comparand != null && __stack != null && __stack.Count > 0)  
				{
					var ambient = __stack.Peek();
					if (Object.ReferenceEquals(ambient, comparand))
					{
						__stack.Pop();
						return true;
					}
				}
				return false;
			}

			internal static ContextFlow Push(ContextFlow ambient)
			{
				if (__stack == null) __stack = new Stack<ContextFlow>();
				__stack.Push(ambient);
				return ambient;
			}										
		}

		abstract class Forker
		{
			public object Item;
			public abstract void Fork(ContextFlow context);
		}
		class Forker<T> : Forker
			where T : IParallelShared
		{
			public Forker(object item)
			{
				this.Item = item;
			}
			public override void Fork(ContextFlow context)
			{
				T fork = (T)((T)Item).ParallelShare();
				context.Cleanup(fork);
				context.PerformPush<T>(fork);
			}
		}

		int _disposers = 1; // always an implicit disposer.
		ConcurrentDictionary<Type, Stack<Forker>> _stacks = new ConcurrentDictionary<Type, Stack<Forker>>();
		List<IDisposable> _cleanup;

		/// <summary>
		/// Prepares a copy of the context for use in thread-pool and background threads.
		/// </summary>
		/// <returns>an instance suitable for use as a background thread's ambient context</returns>
		public static ContextFlow ForkAmbient()
		{
			ContextFlow ambient;
			return (Ambient.TryPeek(out ambient)) ? new ContextFlow(ambient) : default(ContextFlow);
		}

		/// <summary>
		/// Attaches the given context to the current thread.
		/// </summary>
		/// <param name="ambient">a new ambient context</param>
		/// <returns>the provided context</returns>
		public static ContextFlow EnsureAmbient(ContextFlow ambient)
		{
			if (ambient != null)
			{
				Ambient.Push(ambient);
				return ambient;
			}
			return ContextFlow.Current;
		}

		/// <summary>
		/// Gets the current "ambient" context.
		/// </summary>
		public static ContextFlow Current
		{
			get
			{
				ContextFlow ambient;
				return (Ambient.TryPeek(out ambient)) ? ambient : default(ContextFlow);
			}
		}

		private ContextFlow(ContextFlow other)
		{
			Cleanup(other);
			other.ForkContext(this);
		}

		private ContextFlow()
		{
			Ambient.Push(this);
		}

		/// <summary>
		/// Performs the disposal.
		/// </summary>
		/// <param name="disposing">indicates whether the context is disposing</param>
		/// <returns><em>true</em> if disposed as a result of the call; otherwise <em>false</em></returns>
		protected override bool PerformDispose(bool disposing)
		{
			if (disposing && Interlocked.Decrement(ref _disposers) > 0)
			{
				return false;
			}
			if (disposing && !Ambient.TryPop(this))
			{
				var message = "Context disposed out of order. Thar be shenanigans in the disposery!";
				try
				{
					OnTraceEvent(TraceEventType.Warning, message); 
				}
				catch (Exception)
				{ 
				}
			}
			if (disposing)
			{
				if (_cleanup != null)
				{
					foreach (var d in _cleanup)
					{
						var dd = d;
						Util.Dispose(ref dd);
					}
				}
			}
			return true;
		}

		private ContextFlow ForkContext(ContextFlow fork)
		{
			if (IsDisposed) throw new ObjectDisposedException(typeof(ContextFlow).FullName);

			Interlocked.Increment(ref _disposers);
			foreach (var it in _stacks)
			{
				if (it.Value.Count > 0)
				{
					it.Value.Peek().Fork(fork);
				}
			}
			return this;
		}

		/// <summary>
		/// Pushes an instance of T onto the context.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="ambient">an instance</param>
		public static void Push<T>(T ambient)
			where T : IParallelShared
		{
			ContextFlow context;
			if (!Ambient.TryPeek(out context))
			{
				context = new ContextFlow();
			}
			context.PerformPush<T>(ambient);
		}

		private void PerformPush<T>(T ambient)
			where T : IParallelShared
		{
			Stack<Forker> stack = _stacks.GetOrAdd(typeof(T), t => new Stack<Forker>());
			stack.Push(new Forker<T>(ambient));
		}

		/// <summary>
		/// Tries to peek at an ambient instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="ambient">variable where the instance will be returned upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		public static bool TryPeek<T>(out T ambient)
			where T : IParallelShared
		{
			ContextFlow context;
			if (Ambient.TryPeek(out context))
			{
				return context.PerformTryPeek(out ambient);
			}
			ambient = default(T);
			return false;
		}

		private bool PerformTryPeek<T>(out T ambient)
			where T : IParallelShared
		{
			Stack<Forker> stack;
			if (_stacks.TryGetValue(typeof(T), out stack) && stack.Count > 0)
			{
				ambient = (T)stack.Peek().Item;
				return true;
			}
			ambient = default(T);
			return false;
		}

		/// <summary>
		/// Tries to pop at an ambient instance of type T off the context.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="comparand">an instance for comparison</param>
		/// <returns>true if successful; otherwise false</returns>
		public static bool TryPop<T>(T comparand)
			where T : IParallelShared
		{
			ContextFlow context;
			if (Ambient.TryPeek(out context))
			{
				return context.PerformTryPop(comparand);
			}
			return false;
		}

		private bool PerformTryPop<T>(T comparand)
			where T : IParallelShared
		{
			Stack<Forker> stack;
			if (_stacks.TryGetValue(typeof(T), out stack) && stack.Count > 0)
			{
				return Object.ReferenceEquals(comparand, stack.Pop().Item);
			}
			return false;
		}

		internal void Cleanup(object fork)
		{
			if (fork is IDisposable)
			{
				var list = Util.NonBlockingLazyInitializeVolatile(ref _cleanup);
				list.Add(fork as IDisposable);
			}			
		}
	}
}
