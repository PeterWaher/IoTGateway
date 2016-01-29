using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents a range in a ranged HTTP request or response.
	/// </summary>
	public class ByteRangeInterval
	{
		private long? first;
		private long? last;
		private ByteRangeInterval next;

		/// <summary>
		/// Represents a range interval in a ranged HTTP request or response.
		/// </summary>
		/// <param name="First">First byte of interval, if provided. If not provided, the interval represents the last <paramref name="Last"/>
		/// number of bytes of the resource.</param>
		/// <param name="Last">Last byte of interval, inclusive, if provided. If not provided, the interval ends at the end of the resource.</param>
		public ByteRangeInterval(long? First, long? Last)
		{
			this.first = First;
			this.last = Last;
			this.next = null;
		}

		/// <summary>
		/// First byte of interval, if provided. If not provided, the interval represents the last <paramref name="Last"/>
		/// number of bytes of the resource.
		/// </summary>
		public long? First
		{
			get { return this.first; }
		}

		/// <summary>
		/// Last byte of interval, inclusive, if provided. If not provided, the interval ends at the end of the resource.
		/// </summary>
		public long? Last
		{
			get { return this.last; }
		}

		/// <summary>
		/// Next segment.
		/// </summary>
		public ByteRangeInterval Next
		{
			get { return this.next; }
			internal set { this.next = value; }
		}

		/// <summary>
		/// Calculates the number of bytes spanned by the interval.
		/// </summary>
		/// <param name="TotalLength">Total number of bytes of content entity.</param>
		/// <returns>Number of bytes spanned by interval.</returns>
		public long GetIntervalLength(long TotalLength)
		{
			if (!this.first.HasValue)
				return Math.Min(this.last.Value, TotalLength);
			else if (!this.last.HasValue)
				return TotalLength - this.first.Value;
			else
				return this.last.Value - this.first.Value + 1;
		}

	}
}
