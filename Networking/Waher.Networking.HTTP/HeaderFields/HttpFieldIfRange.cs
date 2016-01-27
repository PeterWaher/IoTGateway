using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-Range HTTP Field header. (RFC 2616, §14.27)
	/// </summary>
	public class HttpFieldIfRange : HttpField
	{
		/// <summary>
		/// If-Range HTTP Field header. (RFC 2616, §14.27)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfRange(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
