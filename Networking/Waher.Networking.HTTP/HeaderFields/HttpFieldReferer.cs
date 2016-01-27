using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Referer HTTP Field header. (RFC 2616, §14.36)
	/// </summary>
	public class HttpFieldReferer : HttpField
	{
		/// <summary>
		/// Referer HTTP Field header. (RFC 2616, §14.36)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldReferer(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
