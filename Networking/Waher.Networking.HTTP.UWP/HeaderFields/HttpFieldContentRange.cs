using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Range HTTP Field header. (RFC 2616, §14.16)
	/// </summary>
	public class HttpFieldContentRange : HttpField
	{
		private ContentByteRangeInterval interval = null;

		/// <summary>
		/// Content-Range HTTP Field header. (RFC 2616, §14.16)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentRange(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Content byte range.
		/// </summary>
		public ContentByteRangeInterval Interval
		{
			get
			{
				string s = this.Value;
				if (!s.StartsWith("bytes "))
					return null;

				int i = s.IndexOf('-', 6);
				if (i < 0)
					return null;

				if (!long.TryParse(s.Substring(6, i - 6), out long First))
					return null;

				int j = s.IndexOf('/', i + 1);
				if (j < 0)
					return null;

				if (!long.TryParse(s.Substring(i + 1, j - i - 1), out long Last))
					return null;

				if (!long.TryParse(s.Substring(j + 1), out long Total))
					return null;

				this.interval = new ContentByteRangeInterval(First, Last, Total);

				return this.interval;
			}
		}
	}
}
