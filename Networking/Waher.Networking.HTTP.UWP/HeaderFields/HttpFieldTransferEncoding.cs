using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Transfer-Encoding HTTP Field header. (RFC 2616, §14.41)
	/// </summary>
	public class HttpFieldTransferEncoding : HttpField
	{
		/// <summary>
		/// Transfer-Encoding HTTP Field header. (RFC 2616, §14.41)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldTransferEncoding(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
