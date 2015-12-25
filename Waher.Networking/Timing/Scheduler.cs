using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Waher.Events;

namespace Waher.Networking.Timing
{
	/// <summary>
	/// Class that can be used to schedule events in time. It uses a separate thread to execute scheduled events.
	/// 
	/// If events risk taking a lot of processor time, they should be executed in their separate threads instead of the scheduler thread. 
	/// Otherwise, they may affect the timing of other scheduled events.
	/// 
	/// If no events are scheduled the execution thread is terminated, and recreated when new events are scheduled.
	/// </summary>
	public class Scheduler : IDisposable
	{
		private SortedDictionary<DateTime, ScheduledEvent> events = new SortedDictionary<DateTime, ScheduledEvent>();
		private AutoResetEvent eventsUpdated = new AutoResetEvent(false);
		private ManualResetEvent terminated = new ManualResetEvent(false);
		private Random gen = new Random();
		private ThreadPriority priority;
		private Thread thread = null;
		private string name;

		/// <summary>
		/// Class that can be used to schedule events in time. It uses a separate thread to execute scheduled events.
		/// 
		/// If events risk taking a lot of processor time, they should be executed in their separate threads instead of the scheduler thread. 
		/// Otherwise, they may affect the timing of other scheduled events.
		/// 
		/// If no events are scheduled the execution thread is terminated, and recreated when new events are scheduled.
		/// </summary>
		/// <param name="Priority">Priority of scheduler execution thread.</param>
		/// <param name="Name">Name of scheduler execution thread.</param>
		public Scheduler(ThreadPriority Priority, string Name)
		{
			this.priority = Priority;
			this.name = Name;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.terminated.Set();
		}

		/// <summary>
		/// Adds an event.
		/// </summary>
		/// <param name="When">When to execute the event.</param>
		/// <param name="Callback">Method called when event is to be executed.</param>
		/// <param name="State">State object bassed to <paramref name="Callback"/>.</param>
		/// <returns>Time when event was scheduled. May differ from <paramref name="When"/> by a few ticks, to make sure the timestamp is unique.</returns>
		public DateTime Add(DateTime When, ParameterizedThreadStart Callback, object State)
		{
			lock (this.events)
			{
				while (this.events.ContainsKey(When))
					When = When.AddTicks(this.gen.Next(1, 10));

				this.events[When] = new ScheduledEvent(When, Callback, State);
				this.eventsUpdated.Set();

				if (this.thread == null)
				{
					this.thread = new Thread(this.ExecutionThread);
					this.thread.Priority = this.priority;
					this.thread.Name = this.name;
					this.thread.Start();
				}
			}

			return When;
		}

		/// <summary>
		/// Removes an event scheduled for a given point in time.
		/// 
		/// NOTE: It is important to use the timestamp returned when calling <see cref="Add"/>, not the original value, as these
		/// might differ by a few ticks to make all timestamps unique.
		/// </summary>
		/// <param name="When">Timstamp to remove.</param>
		/// <returns>If the event was found and removed.</returns>
		public bool Remove(DateTime When)
		{
			lock (this.events)
			{
				if (this.events.Remove(When))
				{
					this.eventsUpdated.Set();
					return true;
				}
			}

			return false;
		}

		private void ExecutionThread()
		{
			WaitHandle[] Handles = new WaitHandle[] { this.eventsUpdated, this.terminated };
			ScheduledEvent Next = null;
			DateTime Now;
			TimeSpan TimeToWait;
			int Milliseconds;
			bool Found;

			try
			{
				while (true)
				{
					lock (this.events)
					{
						Found = false;
						foreach (ScheduledEvent Event in this.events.Values)
						{
							Next = Event;
							Found = true;
							break;
						}

						if (Found)
							this.events.Remove(Next.When);
						else
						{
							this.thread = null;
							break;
						}
					}

					Now = DateTime.Now;
					if (Next.When > Now)
					{
						TimeToWait = Next.When - Now;
						Milliseconds = (int)(TimeToWait.TotalMilliseconds + 0.5);

						switch (WaitHandle.WaitAny(Handles, Milliseconds))
						{
							case 0:
								lock (this.events)
								{
									Now = Next.When;

									while (this.events.ContainsKey(Now))
										Now = Now.AddTicks(this.gen.Next(1, 10));

									this.events[Now] = new ScheduledEvent(Now, Next.EventMethod, Next.State);
								}
								continue;

							case 1:
								lock (this.events)
								{
									this.events.Clear();
								}
								return;
						}
					}

					try
					{
						Next.EventMethod(Next.State);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

	}
}
