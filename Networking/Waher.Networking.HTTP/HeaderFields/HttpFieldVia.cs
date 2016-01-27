using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Via HTTP Field header. (RFC 2616, §14.45)
	/// </summary>
	public class HttpFieldVia : HttpField
	{
		/// <summary>
		/// Via HTTP Field header. (RFC 2616, §14.45)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldVia(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
