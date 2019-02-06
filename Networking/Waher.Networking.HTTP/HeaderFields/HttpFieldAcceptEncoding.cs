using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
	/// </summary>
	public class HttpFieldAcceptEncoding : HttpFieldAcceptRecords
	{
		/// <summary>
		/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptEncoding(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
