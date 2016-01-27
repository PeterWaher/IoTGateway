using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Expect HTTP Field header. (RFC 2616, §14.20)
	/// </summary>
	public class HttpFieldExpect : HttpField
	{
		/// <summary>
		/// Expect HTTP Field header. (RFC 2616, §14.20)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldExpect(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
