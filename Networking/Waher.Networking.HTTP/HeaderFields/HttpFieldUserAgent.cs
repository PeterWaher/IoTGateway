using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// UserAgent HTTP Field header. (RFC 2616, §14.43)
	/// </summary>
	public class HttpFieldUserAgent : HttpField
	{
		/// <summary>
		/// UserAgent HTTP Field header. (RFC 2616, §14.43)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldUserAgent(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
