using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Events.Statistics
{
	/// <summary>
	/// Contains statistical information about one item.
	/// </summary>
	public class Statistic
	{
		private long count;
		private DateTime first;
		private DateTime last;

		/// <summary>
		/// Contains statistical information about one item.
		/// </summary>
		/// <param name="InitialCount">Initial count.</param>
		public Statistic(long InitialCount)
		{
			this.first = this.last = DateTime.Now;
			this.count = InitialCount;
		}

		/// <summary>
		/// Increments the counter.
		/// </summary>
		public void Inc()
		{
			this.count++;
			this.last = DateTime.Now;
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
