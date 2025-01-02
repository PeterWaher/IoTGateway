using System;

namespace Waher.Events.Statistics
{
	/// <summary>
	/// Contains statistical information about one item.
	/// </summary>
	public class Statistic
	{
		private readonly DateTime first;
		private DateTime last;
		private long count;

		/// <summary>
		/// Contains statistical information about one item.
		/// </summary>
		/// <param name="InitialCount">Initial count.</param>
		public Statistic(long InitialCount)
		{
			this.first = this.last = DateTime.UtcNow;
			this.count = InitialCount;
		}

		/// <summary>
		/// Increments the counter.
		/// </summary>
		public void Inc()
		{
			this.count++;
			this.last = DateTime.UtcNow;
		}

		/// <summary>
		/// Counter
		/// </summary>
		public long Count => this.count;

		/// <summary>
		/// First event.
		/// </summary>
		public DateTime First => this.first;

		/// <summary>
		/// Last event.
		/// </summary>
		public DateTime Last => this.last;
	}
}
