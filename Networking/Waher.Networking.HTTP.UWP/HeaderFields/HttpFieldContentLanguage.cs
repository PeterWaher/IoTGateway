using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Language HTTP Field header. (RFC 2616, §14.12)
	/// </summary>
	public class HttpFieldContentLanguage : HttpField
	{
		/// <summary>
		/// Content-Language HTTP Field header. (RFC 2616, §14.12)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentLanguage(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
