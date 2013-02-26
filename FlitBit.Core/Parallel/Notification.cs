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

		static int __recordID = 0;

		internal struct NotifyRecord
		{
			public int ID;
			public IAsyncResult Async;
			public WaitHandle Handle;
			public Action Handler;
		}
		readonly Reactor<NotifyRecord> _invoker;
		readonly Notifier _notifier;
		readonly ConcurrentQueue<NotifyRecord> _incomming = new ConcurrentQueue<NotifyRecord>();

		private Notification()
		{
			_invoker = new Reactor<NotifyRecord>((self, rec) => rec.Handler());
			_notifier = new Notifier(true, _incomming, _invoker);
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
				ID = Interlocked.Increment(ref __recordID),
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
			_notifier.Wake(null);
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
			readonly Reactor<NotifyRecord> _notifier;
			readonly int _id;
			IFuture<bool> _alert;
			Thread _waiter;
			ManualResetEvent _signal;
			IEnumerable<NotifyRecord> _items;
			readonly ConcurrentQueue<NotifyRecord> _incomming;
			
			internal Notifier(bool leader, ConcurrentQueue<NotifyRecord> incomming, Reactor<NotifyRecord> notifier)
			{
				_incomming = incomming;
				_notifier = notifier;
				_id = Interlocked.Increment(ref __identityCount);
				if (leader)
				{
					_waiter = new Thread(PerformLeaderLogic);
				}
				else
				{
					_waiter = new Thread(PerformFollowerLogic);
				}
				_signal = new ManualResetEvent(false);
				_waiter.Start();
			}

			internal int ID { get { return _id; } }

			internal void Wake(IFuture<bool> alert)
			{
				if (!IsDisposed && _status.IsLessThan(State.Disposing))
				{
					Util.VolatileWrite(ref _alert, alert);
					_signal.Set();
				}
			}

			void PerformLeaderLogic()
			{
				var subordinates = new List<Notifier>();
				while (_status.SetStateIfLessThan(State.Loading, State.Disposing))
				{
					var queue = new Queue<IFuture<bool>>();
					foreach (var sub in subordinates)
					{
						var futr = new Future<bool>();
						sub.Wake(futr);
						queue.Enqueue(futr);
					}
					// Expects all threads to set the future's value.
					while (queue.Count > 0)
					{
						var ea = queue.Dequeue();
						ea.Wait(TimeSpan.MaxValue);
					}

					NotifyRecord rec;
					List<NotifyRecord> records = new List<NotifyRecord>();
					while (_incomming.TryDequeue(out rec))
					{
						if (rec.Async.IsCompleted) _notifier.Push(rec);
						else
						{
							records.Add(rec);
						}
					}
					var arr = records.ToArray();
					int i = 0, j = 0;
					while (j < records.Count)
					{
						if (i == subordinates.Count) subordinates.Add(new Notifier(false, _incomming, _notifier));
						var sub = subordinates[i++];
						int ub = Math.Min(CRecordsPerWaitPeriod, records.Count - j);
						NotifyRecord[] slice = new NotifyRecord[ub];
						Array.Copy(arr, j, slice, 0, ub);
						sub.Push(arr);
						j += ub;
					}
					if (_incomming.Count == 0)
					{
						if (subordinates.Count > i)
						{
							var remove = subordinates[i];
							subordinates.RemoveAt(i);
							remove.Dispose();
						}
						if (_status.SetStateIfLessThan(State.Waiting, State.Disposing))
						{
							_signal.WaitOne();
							_signal.Reset();
						}
					}
				}
			}

			private void Push(IEnumerable<NotifyRecord> items)
			{
				Util.VolatileWrite(ref _items, items);
				_signal.Set();
			}

			void PerformFollowerLogic()
			{
				const int wake = 0;
				const int offset = -1;
				
				while (_status.SetStateIfLessThan(State.Loading, State.Disposing))
				{
					var records = new List<NotifyRecord>();
					if (_items != null)
					{
						foreach (var item in _items)
						{
							if (item.Async.IsCompleted)
							{
								_notifier.Push(item);
							}
							else
							{
								records.Add(item);
							}
						}
						_items = null;
					}
					var waiting = new WaitHandle[] { _signal }
						.Concat(records.Where(r => r.Async.AsyncWaitHandle != null).Select(r => r.Async.AsyncWaitHandle)).ToArray();
					if (_status.SetStateIfLessThan(State.Waiting, State.Disposing))
					{
						int signaled = WaitHandle.WaitAny(waiting);

						if (signaled > wake)
						{
							_status.SetStateIfLessThan(State.Notifying, State.Disposing);
							var it = records[signaled + offset];
							_items = records.Where(r => r.ID != it.ID);
							_notifier.Push(it);
						}
						else
						{
							_signal.Reset();
							var alert = Util.VolatileRead(ref _alert);
							if (alert != null)
							{
								_alert = null;
								foreach (var it in records)
								{
									_incomming.Enqueue(it);
								}
								alert.MarkCompleted(true);
							}
							else
							{
								_items = records;
							}
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
