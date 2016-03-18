using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Timing
{
	/// <summary>
	/// Class that can be used to schedule events in time. It uses the thread pool to execute scheduled events.
	/// </summary>
	public class Scheduler : IDisposable
	{
		private SortedDictionary<DateTime, ScheduledEvent> events = new SortedDictionary<DateTime, ScheduledEvent>();
		private Random gen = new Random();
		private Timer timer = null;
		private object synchObject = new object();

		/// <summary>
		/// Class that can be used to schedule events in time. It uses the thread pool to execute scheduled events.
		/// </summary>
		public Scheduler()
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			lock (this.synchObject)
			{
				if (this.timer != null)
				{
					this.timer.Dispose();
					this.timer = null;
				}

				this.events = null;
			}
		}

		/// <summary>
		/// Adds an event.
		/// </summary>
		/// <param name="When">When to execute the event.</param>
		/// <param name="Callback">Method called when event is to be executed.</param>
		/// <param name="State">State object bassed to <paramref name="Callback"/>.</param>
		/// <returns>Time when event was scheduled. May differ from <paramref name="When"/> by a few ticks, to make sure the timestamp is unique.</returns>
		public DateTime Add(DateTime When, Action<object> Callback, object State)
		{
			lock (this.events)
			{
				while (this.events.ContainsKey(When))
					When = When.AddTicks(this.gen.Next(1, 10));

				this.events[When] = new ScheduledEvent(When, Callback, State);
				this.EventsUpdatedLocked();
			}

			return When;
		}

		private void EventsUpdatedLocked()
		{
			ScheduledEvent Next = null;
			bool Found = false;

			foreach (ScheduledEvent Event in this.events.Values)
			{
				Next = Event;
				Found = true;
				break;
			}

			if (Found)
			{
				DateTime Now = DateTime.Now;

				if (Next.When > Now)
					this.ScheduleNextTaskLocked(Next.When - Now);
				else
					this.StartTasks();
			}
			else
			{
				if (this.timer != null)
				{
					this.timer.Dispose();
					this.timer = null;
				}
			}
		}

		private void ScheduleNextTaskLocked(TimeSpan When)
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			this.timer = new Timer(this.StartTasks, null, When, TimeSpan.MinValue);
		}

		private void StartTasks(object P)
		{
			this.StartTasks();
		}

		private void StartTasks()
		{
			lock (this.synchObject)
			{
				try
				{
					LinkedList<DateTime> ToRemove = new LinkedList<DateTime>();
					DateTime Now = DateTime.Now;

					foreach (ScheduledEvent Event in this.events.Values)
					{
						if (Event.When > Now)
							break;

						Task.Factory.StartNew(Event.EventMethod, Event.State);
						ToRemove.AddLast(Event.When);
					}

					foreach (DateTime When in ToRemove)
						this.events.Remove(When);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
				finally
				{
					this.EventsUpdatedLocked();
				}
			}
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
					this.EventsUpdatedLocked();
					return true;
				}
			}

			return false;
		}

	}
}
