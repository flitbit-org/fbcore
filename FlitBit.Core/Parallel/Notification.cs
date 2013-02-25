#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Utility for notify
	/// </summary>
	public sealed class Notification
	{
		static Lazy<Notification> __singleton = new Lazy<Notification>(() => new Notification(), LazyThreadSafetyMode.ExecutionAndPublication);

		/// <summary>
		/// Accesses the (Lazy) notification instance.
		/// </summary>
		public static Notification Instance { get { return __singleton.Value; } }

		internal struct NotifyRecord
		{
			public IAsyncResult Async;
			public Action Handler;
		}
		readonly Reactor<NotifyRecord> _invoker;
		readonly ConcurrentDictionary<int, Notifier> _notifiers = new ConcurrentDictionary<int, Notifier>();
		readonly ConcurrentQueue<Notifier> _availableNotifiers = new ConcurrentQueue<Notifier>();
		readonly ConcurrentQueue<NotifyRecord> _incomming = new ConcurrentQueue<NotifyRecord>();

		private Notification()
		{
			_invoker = new Reactor<NotifyRecord>((self, rec) => rec.Handler());
		}

		/// <summary>
		/// Schedules a continuation action for after an async result
		/// completes.
		/// </summary>
		/// <param name="async">the async result</param>
		/// <param name="after">the continuation</param>
		public void ContinueWith(IAsyncResult async, Action after)
		{
			Contract.Requires<ArgumentNullException>(async != null);
			Contract.Requires<ArgumentNullException>(after != null);

			var ambient = ContextFlow.ForkAmbient();			
			var record = new NotifyRecord { 
				Async = async,
				Handler = () =>
				{
					using (var scope = ContextFlow.EnsureAmbient(ambient))
					{
						try
						{
							after();
						}
						catch (Exception e)
						{
							Go.NotifyUncaughtException(after.Target, e);
						}
					}
				} 
			};
			_incomming.Enqueue(record);

			Notifier candidate;
			while (_availableNotifiers.TryDequeue(out candidate))
			{
				if (candidate.Wake())
				{
					if (!candidate.IsFull)
					{
						_availableNotifiers.Enqueue(candidate);
					}
					return;
				}
			}
			var notifier = new Notifier(this, _incomming);
			_notifiers.TryAdd(notifier.ID, notifier);
			notifier.Wake();
		}

		void MakeAvailable(Notifier notifier)
		{
			_availableNotifiers.Enqueue(notifier);
		}

		void PerformNotify(NotifyRecord item)
		{
			_invoker.Push(item);
		}

		internal class Notifier : Disposable
		{
			static readonly int CRecordsPerWaitPeriod = 63;
			static int __identityCount = 0;
			enum State
			{
				Initial = 0,
				Loading = 1,
				Waiting = 1 << 2,
				Notifying = 1 << 3,
				Disposing = 1 << 4,
			}
			readonly Status<State> _status = new Status<State>(State.Initial);
			readonly int _id;
			readonly Notification _owner;
			Thread _waiter;
			ManualResetEvent _signal;
			int _count = 0;
			readonly ConcurrentQueue<NotifyRecord> _incomming;

			internal Notifier(Notification owner, ConcurrentQueue<NotifyRecord> incomming)
			{
				_owner = owner;
				_incomming = incomming;
				_id = Interlocked.Increment(ref __identityCount);
				_waiter = new Thread(ListenForSyncObjects);
				_waiter.Start();
				_signal = new ManualResetEvent(false);
			}

			internal int ID { get { return _id; } }

			internal bool Wake()
			{
				if (!IsDisposed && _status.IsLessThan(State.Disposing))
				{
					if (Thread.VolatileRead(ref _count) < CRecordsPerWaitPeriod)
					{
						Interlocked.Increment(ref _count);
						_signal.Set();
						return true;
					}
				}
				return false;
			}

			internal bool IsFull { get { return Thread.VolatileRead(ref _count) == CRecordsPerWaitPeriod; } }

			class WaitRecord
			{
				public NotifyRecord NotifyRecord;
				public WaitHandle WaitHandle;
			}

			void ListenForSyncObjects()
			{
				const int wake = 0;
				
				var records = new List<WaitRecord>();
				records.Add(new WaitRecord { WaitHandle = _signal });

				while (_status.SetStateIfLessThan(State.Loading, State.Disposing))
				{
					NotifyRecord item;
					while (records.Count < CRecordsPerWaitPeriod
						&& _incomming.TryDequeue(out item))
					{
						if (item.Async.IsCompleted) 
						{
							_owner.PerformNotify(item);
						}
						else 
						{
							var handle = item.Async.AsyncWaitHandle;
							if (handle != null)
							{
								records.Add(new WaitRecord { NotifyRecord = item, WaitHandle = handle });
							}
						}
					}
					if (records.Count < CRecordsPerWaitPeriod)
					{
						_owner.MakeAvailable(this);
					}

					Thread.VolatileWrite(ref _count, records.Count);
					var waiting = records.Select(r => r.WaitHandle).ToArray();
					if (_status.SetStateIfLessThan(State.Waiting, State.Disposing))
					{
						int signaled = WaitHandle.WaitAny(waiting);

						if (signaled > wake)
						{
							_status.SetStateIfLessThan(State.Notifying, State.Disposing);
							var it = records[signaled];
							records.Remove(it);
							_owner.PerformNotify(it.NotifyRecord);							
						}
						else
						{
							_signal.Reset();
						}
					}
				}
			}

			protected override bool PerformDispose(bool disposing)
			{
				if (_waiter != null && _waiter.IsAlive)
				{
					_status.SetStateIfLessThan(State.Disposing, State.Disposing);
					_signal.Set();
					_waiter.Join();
					Util.Dispose(ref _signal);
					Util.Dispose(ref _waiter);
				}
				return true;
			}
		}
	}
}
