using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Type HTTP Field header. (RFC 2616, §14.17)
	/// </summary>
	public class HttpFieldContentType : HttpField
	{
		/// <summary>
		/// Content-Type HTTP Field header. (RFC 2616, §14.17)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentType(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
