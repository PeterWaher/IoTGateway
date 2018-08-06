using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-Match HTTP Field header. (RFC 2616, §14.24)
	/// </summary>
	public class HttpFieldIfMatch : HttpStringValueField
	{
		/// <summary>
		/// If-Match HTTP Field header. (RFC 2616, §14.24)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfMatch(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
