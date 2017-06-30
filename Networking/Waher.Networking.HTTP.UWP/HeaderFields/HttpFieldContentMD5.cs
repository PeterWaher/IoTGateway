using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-MD5 HTTP Field header. (RFC 2616, §14.15)
	/// </summary>
	public class HttpFieldContentMD5 : HttpField
	{
		/// <summary>
		/// Content-MD5 HTTP Field header. (RFC 2616, §14.15)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentMD5(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
