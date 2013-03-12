#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   Utility for notify
	/// </summary>
	public sealed class Notification
	{
		static Lazy<Notification> __singleton = new Lazy<Notification>(() => new Notification(),
																																	LazyThreadSafetyMode.ExecutionAndPublication);

		static int __recordID = 0;

		readonly ConcurrentQueue<NotifyRecord> _incomming = new ConcurrentQueue<NotifyRecord>();
		readonly Reactor<NotifyRecord> _invoker = new Reactor<NotifyRecord>((self, rec) => rec.Handler());
		readonly Lazy<Notifier> _notifier;

		Notification() { _notifier = new Lazy<Notifier>(() => new Notifier(_incomming, _invoker), LazyThreadSafetyMode.ExecutionAndPublication); }

		/// <summary>
		///   Accesses the (Lazy) notification instance.
		/// </summary>
		public static Notification Instance
		{
			get { return __singleton.Value; }
		}

		/// <summary>
		///   Schedules a continuation action for after an async result
		///   completes.
		/// </summary>
		/// <param name="async">the async result</param>
		/// <param name="after">the continuation</param>
		public void ContinueWith(IAsyncResult async, Action after)
		{
			Contract.Requires<ArgumentNullException>(async != null);
			Contract.Requires<ArgumentNullException>(after != null);

			var ambient = ContextFlow.ForkAmbient();
			var record = new NotifyRecord
				{
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
			_notifier.Value.Wake();
		}

		internal class Notifier : Disposable
		{
			const int CStatus_None = 0;
			const int CStatus_Ok = 1;
			const int CStatus_Waiting = 2;
			const int CStatus_Repartition = 3;
			const int CStatus_Stopping = 5;
			const int CStatus_Stopped = 6;

			const int CRecordsPerWaitPeriod = 63;
			static int __threadID = 0;

			readonly ManualResetEvent _alert;
			readonly int _id;
			readonly Reactor<NotifyRecord> _notifier;
			readonly ConcurrentQueue<NotifyRecord> _queue;
			readonly AutoResetEvent _signal;
			readonly Thread _waiter;
			int _status = CStatus_None;

			internal Notifier(ConcurrentQueue<NotifyRecord> queue, Reactor<NotifyRecord> notifier)
			{
				_id = Interlocked.Increment(ref __threadID);
				_queue = queue;
				_notifier = notifier;
				_alert = new ManualResetEvent(false);
				_signal = new AutoResetEvent(false);
				_waiter = new Thread(PerformLeaderLogic);
				_waiter.Start();
			}

			internal int ID
			{
				get { return _id; }
			}

			internal void Wake()
			{
				var res = Thread.VolatileRead(ref _status);
				while (res < CStatus_Stopping)
				{
					if (res == CStatus_Repartition)
					{
						break; // if currently repartitioning then no need to wake.
					}
					if (res == CStatus_Waiting
						&& Interlocked.CompareExchange(ref _status, CStatus_Repartition, CStatus_Waiting) == CStatus_Waiting)
					{
						_signal.Set();
						break; // was waiting and we set status to repartitioning.
					}
					// lost race condition, loop to ensure it is still ok...
					res = Thread.VolatileRead(ref _status);
				}
			}

			void PerformLeaderLogic()
			{
				var listeners = new List<NotifierState>();
				try
				{
					int res;
					while ((res = Thread.VolatileRead(ref _status)) < CStatus_Stopping)
					{
						if (res == CStatus_Repartition)
						{
							if (_queue.Count > 0)
							{
								var candidates = new List<NotifierState>();
								foreach (var c in listeners)
								{
									if (c.MarkRepartition())
									{
										candidates.Add(c);
									}
									else
									{
										c.Stop();
									}
								}
								_alert.Set();
								foreach (var c in candidates)
								{
									c.WaitFor(CStatus_Ok);
								}
								_alert.Reset();
								NotifyRecord rec;
								var records = new List<NotifyRecord>();
								while (_queue.TryDequeue(out rec))
								{
									if (rec.Async.IsCompleted)
									{
										_notifier.Push(rec);
									}
									else
									{
										records.Add(rec);
									}
								}
								var arr = records.ToArray();
								int i = 0, j = 0;
								NotifierState sub;
								while (j < records.Count)
								{
									var ub = Math.Min(CRecordsPerWaitPeriod, records.Count - j);
									var slice = new NotifyRecord[ub];
									Array.Copy(arr, j, slice, 0, ub);
									j += ub;
									var items = new List<NotifyRecord>(slice);

									if (i == candidates.Count)
									{
										candidates.Add(sub = new NotifierState
											{
												ID = Interlocked.Increment(ref __threadID),
												Alert = _alert,
												SharedQueue = _queue,
												Notifier = _notifier
											});
										new Thread(PerformFollowerLogic).Start(sub);
										sub.MarkOk(CStatus_None, items);
									}
									else
									{
										sub = candidates[i];
										sub.MarkOk(CStatus_Repartition, items);
									}

									i++;
								}
								_alert.Set();
								_alert.Reset();
								listeners = new List<NotifierState>(candidates);
							}
							Interlocked.CompareExchange(ref _status, CStatus_Ok, CStatus_Repartition);
						}
						else
						{
							if (Interlocked.CompareExchange(ref _status, CStatus_Waiting, res) == res)
							{
								_signal.WaitOne();
							}
						}
					}
				}
				catch (ThreadAbortException)
				{
				}
				catch (Exception e)
				{
					Go.NotifyUncaughtException(this, e);
				}
				Thread.VolatileWrite(ref _status, CStatus_Stopped);
			}

			void PerformFollowerLogic(object state)
			{
				const int wake = 0;
				const int offset = -1;
				var status = (NotifierState) state;
				status.MarkOk(CStatus_None, null);
				try
				{
					int res;
					while ((res = status.Status) < CStatus_Stopping)
					{
						var items = status.Records;
						switch (res)
						{
							case CStatus_Ok:
							case CStatus_Waiting:
								if (items == null || !items.Any())
								{
									status.Alert.WaitOne();
								}
								else
								{
									var waiting = new WaitHandle[] {status.Alert}.Concat(
																																			 from r in items
																																			where r.Async.AsyncWaitHandle != null
																																			select r.Async.AsyncWaitHandle
										)
																															.ToArray();
									if (res == status.MarkWaiting(res))
									{
										var signaled = WaitHandle.WaitAny(waiting);
										if (signaled > wake)
										{
											var it = items[signaled + offset];
											items.RemoveAt(signaled + offset);
											_notifier.Push(it);
										}
									}
								}
								break;
							case CStatus_Repartition:
								if (items != null)
								{
									var q = status.SharedQueue;
									foreach (var it in items)
									{
										q.Enqueue(it);
									}
								}
								status.MarkOk(CStatus_Repartition, null);
								break;
							default:
								break;
						}
					}
				}
				catch (ThreadAbortException)
				{
				}
				catch (Exception e)
				{
					Go.NotifyUncaughtException(this, e);
				}
				Thread.VolatileWrite(ref _status, CStatus_Stopped);
			}

			protected override bool ShouldTrace(TraceEventType eventType) { return eventType <= TraceEventType.Warning; }

			protected override bool PerformDispose(bool disposing)
			{
				if (disposing)
				{
					Thread.VolatileWrite(ref _status, CStatus_Stopping);
					_alert.Set();
					_signal.Set();
					Thread.Yield();
					_signal.Dispose();
					_alert.Dispose();
				}
				return true;
			}

			class NotifierState
			{
				List<NotifyRecord> _records;
				int _status = CStatus_None;

				public int ID { get; set; }
				public ManualResetEvent Alert { get; set; }

				public List<NotifyRecord> Records
				{
					get { return Util.VolatileRead(ref _records); }
				}

				public ConcurrentQueue<NotifyRecord> SharedQueue { get; set; }
				public Reactor<NotifyRecord> Notifier { get; set; }

				public int Status
				{
					get { return Thread.VolatileRead(ref _status); }
				}

				public int MarkOk(int expect, List<NotifyRecord> records)
				{
					var res = Interlocked.CompareExchange(ref _status, CStatus_Ok, expect);
					if (res == expect && records != null)
					{
						Util.VolatileWrite(ref _records, records);
					}
					return res;
				}

				public int MarkWaiting(int expect) { return Interlocked.CompareExchange(ref _status, CStatus_Waiting, expect); }

				public bool MarkRepartition()
				{
					var res = Thread.VolatileRead(ref _status);
					while (res < CStatus_Stopping)
					{
						if (Interlocked.CompareExchange(ref _status, CStatus_Repartition, res) == res)
						{
							return true;
						}
						// lost race condition, loop to ensure it is still ok...
						res = Thread.VolatileRead(ref _status);
					}
					return false;
				}

				public void MarkStopped() { Thread.VolatileWrite(ref _status, CStatus_Stopped); }

				public void Stop()
				{
					var res = Thread.VolatileRead(ref _status);
					if (res < CStatus_Stopping)
					{
						Thread.VolatileWrite(ref _status, CStatus_Stopping);
					}
				}

				internal void WaitFor(int state)
				{
					while (Status != state)
					{
						Thread.Yield();
					}
				}
			}
		}

		internal struct NotifyRecord
		{
			public IAsyncResult Async;
			public WaitHandle Handle;
			public Action Handler;
			public int ID;
		}
	}
}