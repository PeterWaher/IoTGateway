using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept HTTP Field header. (RFC 2616, §14.1)
	/// </summary>
	public class HttpFieldAccept : HttpField
	{
		/// <summary>
		/// Accept HTTP Field header. (RFC 2616, §14.1)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAccept(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
