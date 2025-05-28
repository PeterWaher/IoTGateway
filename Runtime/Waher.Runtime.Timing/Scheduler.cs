using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Timing
{
	/// <summary>
	/// Class that can be used to schedule events in time. It uses a timer to execute tasks at the appointed time. 
	/// If no events are scheduled the timer is terminated, and recreated when new events are scheduled.
	/// </summary>
	public class Scheduler : IDisposable
	{
		private static readonly TimeSpan OnlyOnce = TimeSpan.FromMilliseconds(-1);
		private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

		private readonly SortedDictionary<DateTime, ScheduledEvent> events = new SortedDictionary<DateTime, ScheduledEvent>();
		private readonly Random gen = new Random();
		private Timer timer = null;
		private bool disposed = false;

		/// <summary>
		/// Class that can be used to schedule events in time. It uses a timer to execute tasks at the appointed time. 
		/// If no events are scheduled the timer is terminated, and recreated when new events are scheduled.
		/// </summary>
		public Scheduler()
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				this.timer?.Dispose();
				this.timer = null;

				this.events.Clear();
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
			bool Utc = When.Kind == DateTimeKind.Utc;

			if (Utc)
				When = When.ToLocalTime();

			lock (this.events)
			{
				while (this.events.ContainsKey(When))
					When = When.AddTicks(this.gen.Next(1, 10));

				this.events[When] = new ScheduledEvent(When, Callback, State);
				this.RecalcTimerLocked();
			}

			if (Utc)
				When = When.ToUniversalTime();

			return When;
		}

		/// <summary>
		/// Adds an event.
		/// </summary>
		/// <param name="When">When to execute the event.</param>
		/// <param name="Callback">Method called when event is to be executed.</param>
		/// <param name="State">State object bassed to <paramref name="Callback"/>.</param>
		/// <returns>Time when event was scheduled. May differ from <paramref name="When"/> by a few ticks, to make sure the timestamp is unique.</returns>
		public DateTime Add(DateTime When, Func<object, Task> Callback, object State)
		{
			bool Utc = When.Kind == DateTimeKind.Utc;

			if (Utc)
				When = When.ToLocalTime();

			lock (this.events)
			{
				while (this.events.ContainsKey(When))
					When = When.AddTicks(this.gen.Next(1, 10));

				this.events[When] = new ScheduledEvent(When, Callback, State);
				this.RecalcTimerLocked();
			}

			if (Utc)
				When = When.ToUniversalTime();

			return When;
		}

		private void RecalcTimerLocked()
		{
			this.timer?.Dispose();
			this.timer = null;

			LinkedList<DateTime> ToRemove = null;
			DateTime Now = DateTime.Now;
			TimeSpan TimeLeft;

			foreach (KeyValuePair<DateTime, ScheduledEvent> Event in this.events)
			{
				TimeLeft = Event.Key - Now;
				if (TimeLeft <= TimeSpan.Zero)
				{
					if (ToRemove is null)
						ToRemove = new LinkedList<DateTime>();

					ToRemove.AddLast(Event.Key);

					Task.Run(Event.Value.Execute);
				}
				else
				{
					if (TimeLeft < OneDay)
						this.timer = new Timer(this.TimerElapsed, null, TimeLeft, OnlyOnce);
					else
						this.timer = new Timer(this.TimerElapsed, null, OneDay, OnlyOnce);

					break;
				}
			}

			if (!(ToRemove is null))
			{
				foreach (DateTime TP in ToRemove)
					this.events.Remove(TP);
			}
		}

		private void TimerElapsed(object P)
		{
			try
			{
				lock (this.events)
				{
					this.RecalcTimerLocked();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Clears all pending events.
		/// </summary>
		public void Clear()
		{
			try
			{
				lock (this.events)
				{
					this.events.Clear();
					this.RecalcTimerLocked();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Removes an event scheduled for a given point in time.
		/// 
		/// NOTE: It is important to use the timestamp returned when calling Add, not the original value, as these
		/// might differ by a few ticks to make all timestamps unique.
		/// </summary>
		/// <param name="When">Timstamp to remove.</param>
		/// <returns>If the event was found and removed.</returns>
		public bool Remove(DateTime When)
		{
			if (When.Kind == DateTimeKind.Utc)
				When = When.ToLocalTime();

			lock (this.events)
			{
				if (this.events.Remove(When))
				{
					this.RecalcTimerLocked();
					return true;
				}
			}

			return false;
		}

	}
}
