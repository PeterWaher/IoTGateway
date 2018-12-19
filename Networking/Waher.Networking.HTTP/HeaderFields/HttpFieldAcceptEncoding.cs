using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
	/// </summary>
	public class HttpFieldAcceptEncoding : HttpField
	{
		private AcceptRecord[] records = null;

		/// <summary>
		/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptEncoding(string Key, string Value)
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
				if (this.records is null)
					this.records = HttpFieldAccept.Parse(this.Value);

				return this.records;
			}
		}
	}
}
