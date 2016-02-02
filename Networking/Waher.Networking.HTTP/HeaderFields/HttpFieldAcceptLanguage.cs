using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept-Language HTTP Field header. (RFC 2616, §14.4)
	/// </summary>
	public class HttpFieldAcceptLanguage : HttpField
	{
		private AcceptRecord[] records = null;

		/// <summary>
		/// Accept-Language HTTP Field header. (RFC 2616, §14.4)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptLanguage(string Key, string Value)
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
