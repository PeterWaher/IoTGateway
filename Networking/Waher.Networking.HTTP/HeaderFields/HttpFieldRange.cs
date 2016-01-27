using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Range HTTP Field header. (RFC 2616, §14.35)
	/// </summary>
	public class HttpFieldRange : HttpField
	{
		/// <summary>
		/// Range HTTP Field header. (RFC 2616, §14.35)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldRange(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
