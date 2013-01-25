#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core.Properties;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// A parallel reactor is used to efficiently trigger actions in parallel in
	/// response to items being pushed to the reactor.
	/// </summary>
	/// <typeparam name="TItem">item type TItem</typeparam>
	public sealed class Reactor<TItem>
	{
		/// <summary>
		/// The default options used by reactors when none are given to the constructor.
		/// </summary>
		public static readonly ReactorOptions DefaultOptions = new ReactorOptions();
				
		readonly ConcurrentQueue<TItem> _queue = new ConcurrentQueue<TItem>();
		readonly Action<Reactor<TItem>, TItem> _reactor;
		readonly Status<ReactorState> _state;
		readonly ReactorOptions _options;
		readonly WaitCallback _backgroundReactor;
		event EventHandler<ReactorExceptionArgs> _uncaughtException;
		int _backgroundWorkers = 0;

		/// <summary>
		/// Creates a new instance with the default options.
		/// </summary>
		/// <param name="reactor">the reactor's action</param>
		public Reactor(Action<Reactor<TItem>, TItem> reactor)
			: this(reactor, DefaultOptions)
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="reactor">the reactor's action</param>
		/// <param name="options">options</param>
		public Reactor(Action<Reactor<TItem>, TItem> reactor, ReactorOptions options)
		{
			Contract.Requires<ArgumentNullException>(reactor != null);

			_reactor = reactor;
			_options = options ?? DefaultOptions;
			_backgroundReactor = (_options.YieldBusyReactor) ? new WaitCallback(Background_Reactor_With_Yield) : new WaitCallback(Background_Reactor);
		}

		/// <summary>
		/// Determines if the reactor is empty. Empty means there are no items
		/// that have not already been reacted too.
		/// </summary>
		public bool IsEmpty {	get { return _queue.IsEmpty; } }

		/// <summary>
		/// Indicates whether the reactor is idle.
		/// </summary>
		public bool IsIdle { get { return _state.CurrentState == ReactorState.Idle; } }
		/// <summary>
		/// Indicates whether the reactor is active.
		/// </summary>
		public bool IsActive { get { return _state.CurrentState == ReactorState.Active; } }
		/// <summary>
		/// Indicates whether the reactor is suspending.
		/// </summary>
		public bool IsSuspendedSignaled { get { return _state.CurrentState == ReactorState.SuspendSignaled; } }
		/// <summary>
		/// Indicates whether the reactor is suspended.
		/// </summary>
		public bool IsSuspended { get { return _state.CurrentState == ReactorState.Suspended; } }
		/// <summary>
		/// Indicates whethe the reactor is stopping.
		/// </summary>
		public bool IsStopSignaled { get { return _state.CurrentState == ReactorState.StopSignaled; } }
		/// <summary>
		/// Indicates whether the reactor is stopped.
		/// </summary>
		public bool IsStopped { get { return _state.CurrentState == ReactorState.Stopped; } }

		/// <summary>
		/// Gets the reactor's options.
		/// </summary>
		public ReactorOptions Options { get { return _options; } }

		/// <summary>
		/// Starts a reactor that has been suspended.
		/// </summary>
		/// <returns>the reactor (for chaining)</returns>
		public Reactor<TItem> Resume()
		{
			var state = _state.CurrentState;
			if (state > ReactorState.Suspended)
				throw new InvalidOperationException(Resources.Err_ReactorStopped);

			if (_state.TryTransition(ReactorState.Idle, ReactorState.Idle, ReactorState.Active, ReactorState.Suspended, ReactorState.SuspendSignaled))
			{
				var workers = Thread.VolatileRead(ref _backgroundWorkers);
				while (_queue.Count > workers &&
					workers < _options.MaxDegreeOfParallelism)
				{
					ThreadPool.QueueUserWorkItem(this._backgroundReactor);
					Thread.Sleep(0);
					workers = Thread.VolatileRead(ref _backgroundWorkers);
				}
			}
			return this;
		}

		/// <summary>
		/// Stops a reactor. Once stopped a reactor cannot be restarted.
		/// </summary>
		/// <returns>the reactor (for chaining)</returns>
		public Reactor<TItem> Stop()
		{
			_state.SetStateIfLessThan(ReactorState.StopSignaled, ReactorState.StopSignaled);
			return this;
		}

		/// <summary>
		/// Suspends a reactor. Suspended reactors may be resumed.
		/// </summary>
		/// <returns>the reactor (for chaining)</returns>
		public Reactor<TItem> Suspend()
		{
			_state.SetStateIfLessThan(ReactorState.SuspendSignaled, ReactorState.SuspendSignaled);
			return this;
		}

		/// <summary>
		/// Pushes a new item to the reactor.
		/// </summary>
		/// <param name="item">an item</param>
		/// <returns>the reactor (for chaining)</returns>
		public Reactor<TItem> Push(TItem item)
		{
			if (_state.IsGreaterThan(ReactorState.Suspended))
				throw new InvalidOperationException(Resources.Err_ReactorStopped);

			_queue.Enqueue(item);

			if (Thread.VolatileRead(ref _backgroundWorkers) < _options.MaxDegreeOfParallelism)
			{
				ThreadPool.QueueUserWorkItem(this._backgroundReactor);
			}
			return this;
		}

		/// <summary>
		/// Event fired when uncaught exceptions are encountered by the reactor.
		/// </summary>
		public event EventHandler<ReactorExceptionArgs> UncaughtException
		{
			add { _uncaughtException += value; }
			remove { _uncaughtException -= value; }
		}

		private bool OnUncaughtException(Exception err)
		{
			if (_uncaughtException == null) return false;

			var args = new ReactorExceptionArgs(err);
			_uncaughtException(this, args);
			return args.Rethrow;
		}

		private void Background_Reactor_With_Yield(object unused_state)
		{
			var threadID = Thread.CurrentThread.ManagedThreadId;
			var itemsHandled = 0;
			var state = _state.CurrentState;

			try
			{
				var count = Interlocked.Increment(ref _backgroundWorkers);
				if (count <= _options.MaxDegreeOfParallelism)
				{					
					if (state == ReactorState.Idle)
						_state.TryTransition(ReactorState.Active, ReactorState.Idle);

					TItem item;
					// Continue until signaled or no more items in queue...
					while (_state.IsLessThan(ReactorState.SuspendSignaled)
						&& _queue.TryDequeue(out item))
					{
						itemsHandled++;
						try
						{
							_reactor(this, item);
						}
						catch (Exception e)
						{
							if (OnUncaughtException(e)) throw;
						}
						if (itemsHandled >= _options.YieldFrequency)
						{
							ThreadPool.QueueUserWorkItem(this._backgroundReactor);
							break;
						}
					}
				}
			}
			finally
			{
				if (Interlocked.Decrement(ref _backgroundWorkers) == 0)
				{
					state = _state.CurrentState;
					if (state == ReactorState.Active)
						_state.TryTransition(ReactorState.Idle, ReactorState.Active);
					else if (state == ReactorState.SuspendSignaled)
						_state.TryTransition(ReactorState.Suspended, ReactorState.SuspendSignaled);
					else if (state == ReactorState.StopSignaled)
						_state.TryTransition(ReactorState.Stopped, ReactorState.StopSignaled);
				}
			}
		}

		private void Background_Reactor(object unused_state)
		{
			var threadID = Thread.CurrentThread.ManagedThreadId;
			var itemsHandled = 0;
			var state = _state.CurrentState;

			try
			{
				var count = Interlocked.Increment(ref _backgroundWorkers);
				if (count <= _options.MaxDegreeOfParallelism)
				{
					if (state == ReactorState.Idle)
						_state.TryTransition(ReactorState.Active, ReactorState.Idle);

					TItem item;
					// Continue until signaled or no more items in queue...
					while (_state.IsLessThan(ReactorState.SuspendSignaled)
						&& _queue.TryDequeue(out item))
					{
						itemsHandled++;
						try
						{
							_reactor(this, item);
						}
						catch (Exception e)
						{
							if (OnUncaughtException(e)) throw;
						}
					}
				}
			}
			finally
			{
				if (Interlocked.Decrement(ref _backgroundWorkers) == 0)
				{
					state = _state.CurrentState;
					if (state == ReactorState.Active)
						_state.TryTransition(ReactorState.Idle, ReactorState.Active);
					else if (state == ReactorState.SuspendSignaled)
						_state.TryTransition(ReactorState.Suspended, ReactorState.SuspendSignaled);
					else if (state == ReactorState.StopSignaled)
						_state.TryTransition(ReactorState.Stopped, ReactorState.StopSignaled);
				}
			}
		}
	}
}
