using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// From HTTP Field header. (RFC 2616, §14.22)
	/// </summary>
	public class HttpFieldFrom : HttpField
	{
		/// <summary>
		/// From HTTP Field header. (RFC 2616, §14.22)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldFrom(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
