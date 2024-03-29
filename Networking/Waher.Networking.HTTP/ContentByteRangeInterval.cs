﻿using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Represents a content range in a ranged HTTP request or response.
	/// </summary>
	public class ContentByteRangeInterval
	{
		private readonly long first;
		private readonly long last;
		private readonly long total;

		/// <summary>
		/// Represents a content range interval in a ranged HTTP request or response.
		/// </summary>
		/// <param name="First">First byte of interval.</param>
		/// <param name="Last">Last byte of interval.</param>
		/// <param name="Total">Total number of bytes of content entity.</param>
		public ContentByteRangeInterval(long First, long Last, long Total)
		{
			this.first = First;
			this.last = Last;
			this.total = Total;
		}

		/// <summary>
		/// First byte of interval.
		/// </summary>
		public long First => this.first;

		/// <summary>
		/// Last byte of interval, inclusive.
		/// </summary>
		public long Last => this.last;

		/// <summary>
		/// Total number of bytes of content entity.
		/// </summary>
		public long Total => this.total;

		/// <inheritdoc/>
		public override string ToString()
		{
			return ContentRangeToString(this.first, this.last, this.total);
		}

		/// <summary>
		/// Converts the content range to an HTTP header field value string.
		/// </summary>
		/// <param name="First">First byte of content range.</param>
		/// <param name="Last">Last byte of content range.</param>
		/// <param name="Total">Total number of bytes of content entity.</param>
		/// <returns>Content-Range HTTP header field value string.</returns>
		public static string ContentRangeToString(long First, long Last, long Total)
		{
			StringBuilder Output = new StringBuilder();

			Output.Append("bytes ");
			Output.Append(First.ToString());
			Output.Append('-');
			Output.Append(Last.ToString());
			Output.Append('/');
			Output.Append(Total.ToString());

			return Output.ToString();
		}

	}
}
