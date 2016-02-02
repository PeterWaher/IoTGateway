using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept-Charset HTTP Field header. (RFC 2616, §14.2)
	/// </summary>
	public class HttpFieldAcceptCharset : HttpField
	{
		private AcceptRecord[] records = null;

		/// <summary>
		/// Accept-Charset HTTP Field header. (RFC 2616, §14.2)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptCharset(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Accept records, sorted by acceptability by the client.
		/// </summary>
		public AcceptRecord[] Records
		{
			get
			{
				if (this.records == null)
					this.records = HttpFieldAccept.Parse(this.Value);

				return this.records;
			}
		}
	}
}
