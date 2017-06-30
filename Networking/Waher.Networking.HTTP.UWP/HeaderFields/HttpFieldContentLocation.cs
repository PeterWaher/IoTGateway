using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Content-Location HTTP Field header. (RFC 2616, §14.14)
	/// </summary>
	public class HttpFieldContentLocation : HttpField
	{
		/// <summary>
		/// Content-Location HTTP Field header. (RFC 2616, §14.14)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldContentLocation(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
