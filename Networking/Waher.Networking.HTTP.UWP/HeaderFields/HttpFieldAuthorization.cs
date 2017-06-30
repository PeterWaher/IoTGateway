using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Authorization HTTP Field header. (RFC 2616, §14.8)
	/// </summary>
	public class HttpFieldAuthorization : HttpField
	{
		/// <summary>
		/// Authorization HTTP Field header. (RFC 2616, §14.8)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAuthorization(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
