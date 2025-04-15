using System;
using System.Collections.Generic;
using System.Net;
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
		private Dictionary<string, string> otherProperties = null;
		private DateTimeOffset? expires = null;
		private readonly string name;
		private readonly string value;
		private readonly string domain = null;
		private readonly string path = null;
		private readonly int? maxAgeSeconds = null;
		private readonly bool secure = false;
		private readonly bool httpOnly = false;

		/// <summary>
		/// Contains information about a cookie, as defined in RFC 6265.
		/// https://tools.ietf.org/html/rfc6265
		/// </summary>
		private Cookie()
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
			this.value = WebUtility.UrlEncode(Value);
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
			this.value = WebUtility.UrlEncode(Value);
			this.domain = Domain;
			this.path = Path;
			this.maxAgeSeconds = MaxAgeSeconds;
			this.secure = Secure;
			this.httpOnly = HttpOnly;
		}

		/// <summary>
		/// Any other properties registered on the cookie.
		/// </summary>
		public IEnumerable<KeyValuePair<string, string>> OtherProperties => (IEnumerable<KeyValuePair<string, string>>)this.otherProperties ?? Array.Empty<KeyValuePair<string, string>>();

		/// <summary>
		/// Name of cookie
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Value of cookie.
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// Domain of cookie.
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// Path of cookie.
		/// </summary>
		public string Path => this.path;

		/// <summary>
		/// When cookie expires.
		/// </summary>
		public DateTimeOffset? Expires => this.expires;

		/// <summary>
		/// Maximum age of cookie, in seconds.
		/// </summary>
		public int? MaxAgeSeconds => this.maxAgeSeconds;

		/// <summary>
		/// If cookie is secure.
		/// </summary>
		public bool Secure => this.secure;

		/// <summary>
		/// If cookie is for HTTP use only.
		/// </summary>
		public bool HttpOnly => this.httpOnly;

		/// <summary>
		/// Creates a Cookie object from a Set-Cookie header field value.
		/// </summary>
		/// <param name="SetCookieFieldValue">Set-Cookie header field value.</param>
		/// <returns>Cookie object representation. If no cookie information available, null is returned.</returns>
		public static Cookie FromSetCookie(string SetCookieFieldValue)
		{
			DateTimeOffset? Expires = null;
			string Name = string.Empty;
			string Value = string.Empty;
			string Domain = null;
			string Path = null;
			string SameSite = null;
			int? MaxAgeSeconds = null;
			bool Secure = false;
			bool HttpOnly = false;
			bool Partitioned = false;
			bool First = true;
			int i;

			foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(SetCookieFieldValue))
			{
				if (First)
				{
					Name = P.Key;
					Value = WebUtility.UrlDecode(P.Value);
					First = false;
				}
				else
				{
					switch (P.Key.ToLower())
					{
						case "domain":
							Domain = P.Value;
							break;

						case "expires":
							if (CommonTypes.TryParseRfc822(P.Value, out DateTimeOffset DTO))
								Expires = DTO;
							else if (int.TryParse(P.Value, out i))
								Expires = DateTimeOffset.UtcNow.AddSeconds(i);
							break;

						case "secure":
							Secure = true;
							break;

						case "httponly":
							HttpOnly = true;
							break;

						case "max-age":
							if (int.TryParse(P.Value, out i))
								MaxAgeSeconds = i;
							break;

						case "partitioned":
							Partitioned = true;
							break;

						case "path":
							Path = P.Value;
							break;

						case "samesite":
							SameSite = P.Value;
							break;
					}
				}
			}

			if (First)
				return null;

			Cookie Result;

			if (MaxAgeSeconds.HasValue)
				Result = new Cookie(Name, Value, Domain, Path, MaxAgeSeconds.Value, Secure, HttpOnly);
			else
				Result = new Cookie(Name, Value, Domain, Path, Expires, Secure, HttpOnly);

			if (Partitioned)
				Result.AddProperty("Partitioned", string.Empty);

			if (!string.IsNullOrEmpty(SameSite))
				Result.AddProperty("SameSite", SameSite);

			return Result;
		}

		/// <summary>
		/// Adds another property on the cookie.
		/// </summary>
		/// <param name="Name">Name of property.</param>
		/// <param name="Value">Value of property.</param>
		public void AddProperty(string Name, string Value)
		{
			if (this.otherProperties is null)
				this.otherProperties = new Dictionary<string, string>();

			this.otherProperties[Name] = Value;
		}

		/// <summary>
		/// Tries to get another property value previously added using <see cref="AddProperty(string, string)"/>.
		/// </summary>
		/// <param name="Name">Name of property.</param>
		/// <param name="Value">Value of property, if found, null otherwise.</param>
		/// <returns>If the property was found.</returns>
		public bool TryGetProperty(string Name, out string Value)
		{
			if (this.otherProperties is null)
			{
				Value = null;
				return false;
			}
			else
				return this.otherProperties.TryGetValue(Name, out Value);
		}

		/// <inheritdoc/>
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

			if (!(this.domain is null))
			{
				Output.Append("; Domain=");
				Output.Append(this.domain);
			}

			if (!(this.path is null))
			{
				Output.Append("; Path=");
				Output.Append(this.path);
			}

			if (this.secure)
				Output.Append("; Secure");

			if (this.httpOnly)
				Output.Append("; HttpOnly");

			if (!(this.otherProperties is null))
			{
				foreach (KeyValuePair<string, string> P in this.otherProperties)
				{
					Output.Append("; ");
					Output.Append(P.Key);

					if (!string.IsNullOrEmpty(P.Value))
					{
						Output.Append('=');
						Output.Append(P.Value);
					}
				}
			}

			return Output.ToString();
		}

		/// <summary>
		/// Converts a <see cref="Cookie"/> to a <see cref="System.Net.Cookie"/>
		/// </summary>
		/// <param name="Cookie">Cookie</param>
		public static explicit operator System.Net.Cookie(Cookie Cookie)
		{
			return new System.Net.Cookie(Cookie.name, Cookie.value, Cookie.path, Cookie.domain);
		}

		/// <summary>
		/// Converts a <see cref="System.Net.Cookie"/> to a <see cref="Cookie"/>
		/// </summary>
		/// <param name="Cookie">Cookie</param>
		public static explicit operator Cookie(System.Net.Cookie Cookie)
		{
			return new Cookie(Cookie.Name, Cookie.Value, Cookie.Domain, Cookie.Path, Cookie.Expires, Cookie.Secure, Cookie.HttpOnly);
		}
	}
}
