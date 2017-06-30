using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Range HTTP Field header. (RFC 2616, §14.35)
	/// </summary>
	public class HttpFieldRange : HttpField
	{
		private ByteRangeInterval firstInterval = null;

		/// <summary>
		/// Range HTTP Field header. (RFC 2616, §14.35)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldRange(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// First byte range interval. If byte ranges are invalid, null is returned.
		/// </summary>
		public ByteRangeInterval FirstInterval
		{
			get
			{
				if (this.firstInterval != null)
					return this.firstInterval;

				string s = this.Value;
				if (!s.StartsWith("bytes="))
					return null;

				string[] Parts = s.Substring(6).Split(',');
				int i, j, c = Parts.Length;
				long? First;
				long? Last;
				long l;
				ByteRangeInterval Prev = null;
				ByteRangeInterval Interval;
				ByteRangeInterval FirstInterval = null;

				for (i = 0; i < c; i++)
				{
					s = Parts[i];
					j = s.IndexOf('-');
					if (j < 0)
						return null;

					if (j == 0)
						First = null;
					else if (long.TryParse(s.Substring(0, j), out l))
						First = l;
					else
						return null;

					if (j == s.Length - 1)
						Last = null;
					else if (long.TryParse(s.Substring(j + 1), out l))
					{
						Last = l;

						if (First.HasValue && First.Value > l)
							return null;
					}
					else
						return null;

					Interval = new ByteRangeInterval(First, Last);
					if (Prev != null)
						Prev.Next = Interval;
					else
						FirstInterval = Interval;

					Prev = Interval;
				}

				this.firstInterval = FirstInterval;

				return FirstInterval;
			}
		}

	}
}
