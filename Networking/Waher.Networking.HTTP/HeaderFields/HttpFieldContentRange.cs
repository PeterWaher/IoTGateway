using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Range HTTP Field header. (RFC 2616, §14.16)
	/// </summary>
	public class HttpFieldContentRange : HttpField
	{
		/// <summary>
		/// Content-Range HTTP Field header. (RFC 2616, §14.16)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentRange(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
