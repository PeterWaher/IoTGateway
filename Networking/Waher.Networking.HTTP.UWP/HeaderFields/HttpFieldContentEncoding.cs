using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Encoding HTTP Field header. (RFC 2616, §14.11)
	/// </summary>
	public class HttpFieldContentEncoding : HttpField
	{
		/// <summary>
		/// Content-Encoding HTTP Field header. (RFC 2616, §14.11)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentEncoding(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
