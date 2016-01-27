using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-Modified-Since HTTP Field header. (RFC 2616, §14.25)
	/// </summary>
	public class HttpFieldIfModifiedSince : HttpField
	{
		/// <summary>
		/// If-Modified-Since HTTP Field header. (RFC 2616, §14.25)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfModifiedSince(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
