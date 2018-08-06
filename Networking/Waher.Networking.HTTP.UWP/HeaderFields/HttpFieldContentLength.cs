using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Length HTTP Field header. (RFC 2616, §14.13)
	/// </summary>
	public class HttpFieldContentLength : HttpField
	{
		private long? contentLength = null;

		/// <summary>
		/// Content-Length HTTP Field header. (RFC 2616, §14.13)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentLength(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Content length, in bytes. If length is invalid, a negative value is returned.
		/// </summary>
		public long ContentLength
		{
			get
			{
				if (this.contentLength.HasValue)
					return this.contentLength.Value;
				else
				{
					if (!long.TryParse(this.Value, out long l))
						this.contentLength = -1;
					else
						this.contentLength = l;

					return l;
				}
			}
		}

	}
}
