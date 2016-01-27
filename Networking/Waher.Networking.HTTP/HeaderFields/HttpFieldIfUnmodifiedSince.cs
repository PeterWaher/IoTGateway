using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// If-Unmodified-Since HTTP Field header. (RFC 2616, §14.28)
	/// </summary>
	public class HttpFieldIfUnmodifiedSince : HttpField
	{
		/// <summary>
		/// If-Unmodified-Since HTTP Field header. (RFC 2616, §14.28)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldIfUnmodifiedSince(string Key, string Value)
			: base(Key, Value)
		{
		}
	}
}
