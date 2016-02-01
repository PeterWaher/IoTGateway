using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains information about a cookie, as defined in RFC 6265.
	/// https://tools.ietf.org/html/rfc6265
	/// </summary>
	public class Cookie
	{
		private string name;
		private string value;
		private string domain = null;
		private string path = null;
		private DateTimeOffset? expires = null;
		private int? maxAgeSeconds = null;
		private bool secure = false;
		private bool httpOnly = false;

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		/// <param name="Name">Cookie name.</param>
		/// <param name="Value">Value of cookie</param>
		/// <param name="Domain">To which domain the cookie belongs.</param>
		/// <param name="Path">Path limitation.</param>
		public Cookie(string Name, string Value, string Domain, string Path)
			: this(Name, Value, Domain, Path, null, false, false)
		{
		}

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		/// <param name="Name">Cookie name.</param>
		/// <param name="Value">Value of cookie</param>
		/// <param name="Domain">To which domain the cookie belongs.</param>
		/// <param name="Path">Path limitation.</param>
		/// <param name="Expires">When cookie expires.</param>
		public Cookie(string Name, string Value, string Domain, string Path, DateTimeOffset? Expires)
			: this(Name, Value, Domain, Path, Expires, false, false)
		{
		}

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		/// <param name="Name">Cookie name.</param>
		/// <param name="Value">Value of cookie</param>
		/// <param name="Domain">To which domain the cookie belongs.</param>
		/// <param name="Path">Path limitation.</param>
		/// <param name="Expires">When cookie expires.</param>
		/// <param name="Secure">If the cookie should only be used over secure channels.</param>
		/// <param name="HttpOnly">If the cookie should only be made available over HTTP requests.</param>
		public Cookie(string Name, string Value, string Domain, string Path, DateTimeOffset? Expires, bool Secure, bool HttpOnly)
		{
			this.name = Name;
			this.value = Value;
			this.domain = Domain;
			this.path = Path;
			this.expires = Expires;
			this.secure = Secure;
			this.httpOnly = HttpOnly;
		}

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		/// <param name="Name">Cookie name.</param>
		/// <param name="Value">Value of cookie</param>
		/// <param name="Domain">To which domain the cookie belongs.</param>
		/// <param name="Path">Path limitation.</param>
		/// <param name="MaxAgeSeconds">Maximum age of cookie in seconds. A relative alternative to the absolut Expires parameter.</param>
		public Cookie(string Name, string Value, string Domain, string Path, int MaxAgeSeconds)
			: this(Name, Value, Domain, Path, MaxAgeSeconds, false, false)
		{
		}

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		/// <param name="Name">Cookie name.</param>
		/// <param name="Value">Value of cookie</param>
		/// <param name="Domain">To which domain the cookie belongs.</param>
		/// <param name="Path">Path limitation.</param>
		/// <param name="MaxAgeSeconds">Maximum age of cookie in seconds. A relative alternative to the absolut Expires parameter.</param>
		/// <param name="Secure">If the cookie should only be used over secure channels.</param>
		/// <param name="HttpOnly">If the cookie should only be made available over HTTP requests.</param>
		public Cookie(string Name, string Value, string Domain, string Path, int MaxAgeSeconds, bool Secure, bool HttpOnly)
		{
			this.name = Name;
			this.value = Value;
			this.domain = Domain;
			this.path = Path;
			this.maxAgeSeconds = MaxAgeSeconds;
			this.secure = Secure;
			this.httpOnly = HttpOnly;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Output = new StringBuilder();

			Output.Append(this.name);
			Output.Append('=');
			Output.Append(this.value);

			if (this.expires.HasValue)
			{
				Output.Append("; Expires=");
				Output.Append(CommonTypes.EncodeRfc822(this.expires.Value));
			}

			if (this.maxAgeSeconds.HasValue)
			{
				Output.Append("; Max-Age=");
				Output.Append(this.maxAgeSeconds.Value.ToString());
			}

			if (this.domain != null)
			{
				Output.Append("; Domain=");
				Output.Append(this.domain);
			}

			if (this.path != null)
			{
				Output.Append("; Path=");
				Output.Append(this.path);
			}

			if (this.secure)
				Output.Append("; Secure");

			if (this.httpOnly)
				Output.Append("; HttpOnly");

			return Output.ToString();
		}
	}
}
