using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Cookie HTTP Field header. (RFC 6265, §5.2)
	/// </summary>
	public class HttpFieldCookie : HttpField
	{
		private KeyValuePair<string, string>[] cookies = null;
		private Dictionary<string, string> valueByName = null;

		/// <summary>
		/// Cookie HTTP Field header. (RFC 6265, §5.2)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldCookie(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Cookies available in the request.
		/// </summary>
		public KeyValuePair<string, string>[] Cookies
		{
			get
			{
				if (this.cookies == null)
					this.cookies = HttpField.ParseFieldValues(this.Value);

				return this.cookies;
			}
		}

		/// <summary>
		/// Gets the value of a cookie. If the cookie is not found, the empty string is returned.
		/// </summary>
		/// <param name="CookieName">Name of cookie.</param>
		/// <returns>Value of cookie, if found, or the empty string, if not found.</returns>
		public string this[string CookieName]
		{
			get
			{
				if (this.valueByName == null)
				{
					this.valueByName = new Dictionary<string, string>();

					foreach (KeyValuePair<string, string> P in this.Cookies)
						this.valueByName[P.Key] = P.Value;
				}

				string Result;

				if (this.valueByName.TryGetValue(CookieName, out Result))
					return Result;
				else
					return string.Empty;
			}
		}
	}
}
