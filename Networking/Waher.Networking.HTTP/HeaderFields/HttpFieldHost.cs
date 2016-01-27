using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Host HTTP Field header. (RFC 2616, §14.23)
	/// </summary>
	public class HttpFieldHost : HttpField
	{
		/// <summary>
		/// Host HTTP Field header. (RFC 2616, §14.23)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldHost(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
