using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-None-Match HTTP Field header. (RFC 2616, §14.26)
	/// </summary>
	public class HttpFieldIfNoneMatch : HttpField
	{
		/// <summary>
		/// If-None-Match HTTP Field header. (RFC 2616, §14.26)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfNoneMatch(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
