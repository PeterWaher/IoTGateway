using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Upgrade-Insecure-Requests HTTP Field header.
	/// https://www.w3.org/TR/upgrade-insecure-requests/
	/// </summary>
	public class HttpFieldUpgradeInsecureRequests : HttpField
	{
		private readonly bool upgrade;

		/// <summary>
		/// Upgrade-Insecure-Requests HTTP Field header.
		/// https://www.w3.org/TR/upgrade-insecure-requests/
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldUpgradeInsecureRequests(string Key, string Value)
			: base(Key, Value)
		{
			this.upgrade = (Value == "1");
		}

		/// <summary>
		/// If the client prefers the request to be upgraded to HTTPS.
		/// </summary>
		public bool Upgrade => this.upgrade;
	}
}
