using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-Range HTTP Field header. (RFC 2616, §14.27)
	/// </summary>
	public class HttpFieldIfRange : HttpField
	{
		private DateTimeOffset? timestamp;

		/// <summary>
		/// If-Range HTTP Field header. (RFC 2616, §14.27)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfRange(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTimeOffset? Timestamp
		{
			get
			{
				if (!this.timestamp.HasValue)
				{
					if (CommonTypes.TryParseRfc822(this.Value, out DateTimeOffset TP))
						this.timestamp = TP;
				}

				return this.timestamp;
			}
		}

	}
}
