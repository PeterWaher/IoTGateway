using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.Vanity;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains information about all fields in an HTTP request header.
	/// </summary>
	public class HttpRequestHeader : HttpHeader
	{
		private Dictionary<string, string> query = null;
		private KeyValuePair<string, string>[] queryParameters = null;
		private HttpFieldAccept accept = null;
		private HttpFieldAcceptCharset acceptCharset = null;
		private HttpFieldAcceptEncoding acceptEncoding = null;
		private HttpFieldAcceptLanguage acceptLanguage = null;
		private HttpFieldAuthorization authorization = null;
		private HttpFieldCookie cookie = null;
		private HttpFieldExpect expect = null;
		private HttpFieldFrom from = null;
		private HttpFieldHost host = null;
		private HttpFieldIfMatch ifMatch = null;
		private HttpFieldIfModifiedSince ifModifiedSince = null;
		private HttpFieldIfNoneMatch ifNoneMatch = null;
		private HttpFieldIfRange ifRange = null;
		private HttpFieldIfUnmodifiedSince ifUnmodifiedSince = null;
		private HttpFieldReferer referer = null;
		private HttpFieldRange range = null;
		private HttpFieldUserAgent userAgent = null;
		private HttpFieldUpgradeInsecureRequests upgradeInsequreRequests = null;
		private HttpField connection = null;
		private HttpField upgrade = null;
		private string method = string.Empty;
		private string resource = string.Empty;
		private string resourcePart = string.Empty;
		private string queryString = string.Empty;
		private string fragment = string.Empty;
		private string uriScheme = string.Empty;
		private double httpVersion = -1;

		/// <summary>
		/// Contains information about all fields in an HTTP request header.
		/// </summary>
		/// <param name="Version">HTTP Version</param>
		public HttpRequestHeader(double Version)
			: base()
		{
			this.httpVersion = Version;
		}

		/// <summary>
		/// Contains information about all fields in an HTTP request header.
		/// </summary>
		/// <param name="Header">HTTP Header.</param>
		/// <param name="VanityResources">Registered vanity resources.</param>
		/// <param name="UriScheme">URI Scheme.</param>
		public HttpRequestHeader(string Header, VanityResources VanityResources, string UriScheme)
			: base(Header, VanityResources)
		{
			this.uriScheme = UriScheme;
		}

		/// <summary>
		/// Contains information about all fields in an HTTP request header.
		/// </summary>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="Resource">Resource.</param>
		/// <param name="Version">HTTP Version.</param>
		/// <param name="UriScheme">URI scheme.</param>
		/// <param name="VanityResources">Registered vanity resource.</param>
		/// <param name="Headers">HTTP Header fields.</param>
		public HttpRequestHeader(string Method, string Resource, string Version, string UriScheme, VanityResources VanityResources,
			params KeyValuePair<string, string>[] Headers)
			: base(Method + " " + Resource + " HTTP/" + Version, VanityResources, Headers)
		{
			this.uriScheme = UriScheme;
		}

		/// <summary>
		/// Parses the first row of an HTTP header.
		/// </summary>
		/// <param name="Row">First row.</param>
		/// <param name="VanityResources">Registered vanity resources.</param>
		protected override void ParseFirstRow(string Row, VanityResources VanityResources)
		{
			int i = Row.IndexOf(' ');
			if (i > 0)
			{
				this.method = Row.Substring(0, i).ToUpper();

				int j = Row.LastIndexOf(' ');
				if (j > 0 && j < Row.Length - 5 && Row.Substring(j + 1, 5) == "HTTP/")
				{
					if (j > i + 1)
						this.SetResource(Row.Substring(i + 1, j - i - 1).Trim(), VanityResources);
					else
						this.SetResource(string.Empty, VanityResources);

					if (!CommonTypes.TryParse(Row.Substring(j + 6), out this.httpVersion))
						this.httpVersion = -1;
				}
			}
		}

		/// <summary>
		/// Sets the resource part of the request.
		/// </summary>
		/// <param name="Resource">Resource part</param>
		/// <param name="VanityResources">Vanity resources.</param>
		public void SetResource(string Resource, VanityResources VanityResources)
		{
			this.resourcePart = Resource;
			VanityResources?.CheckVanityResource(ref this.resourcePart);
			this.resource = this.resourcePart;

			int i = this.resource.IndexOf('?');
			if (i >= 0)
			{
				this.queryString = this.resource.Substring(i + 1);
				this.resource = this.resource.Substring(0, i);

				i = this.queryString.IndexOf('#');
				if (i >= 0)
				{
					this.fragment = this.queryString.Substring(i + 1);
					this.queryString = this.queryString.Substring(0, i);
				}

				this.query = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
				List<KeyValuePair<string, string>> Parameters = new List<KeyValuePair<string, string>>();
				string Key, Name;

				foreach (string Part in this.queryString.Split('&'))
				{
					i = Part.IndexOf('=');
					if (i < 0)
					{
						this.query[Part] = string.Empty;
						Parameters.Add(new KeyValuePair<string, string>(Part, string.Empty));
					}
					else
					{
						Key = Part.Substring(0, i);
						Name = Part.Substring(i + 1);
						this.query[Key] = Name;
						Parameters.Add(new KeyValuePair<string, string>(Key, Name));
					}
				}

				this.queryParameters = Parameters.ToArray();
			}
			else
			{
				i = this.resource.IndexOf('#');
				if (i >= 0)
				{
					this.fragment = this.resource.Substring(i + 1);
					this.resource = this.resource.Substring(0, i);
				}
			}
		}

		/// <summary>
		/// HTTP Method
		/// </summary>
		public string Method
		{
			get => this.method;
			internal set => this.method = value;
		}

		/// <summary>
		/// Resource
		/// </summary>
		public string Resource
		{
			get => this.resource;
			internal set => this.resource = value;
		}

		/// <summary>
		/// Contains original resource part of request.
		/// </summary>
		public string ResourcePart => this.resourcePart;

		/// <summary>
		/// HTTP Version.
		/// </summary>
		public double HttpVersion => this.httpVersion;

		/// <summary>
		/// Query string. To get the values of individual query parameters, use the <see cref="TryGetQueryParameter"/> method.
		/// </summary>
		public string QueryString => this.queryString;

		/// <summary>
		/// Fragment.
		/// </summary>
		public string Fragment => this.fragment;

		/// <summary>
		/// URI scheme.
		/// </summary>
		public string UriScheme
		{
			get => this.uriScheme;
			internal set => this.uriScheme = value;
		}

		/// <summary>
		/// Tries to get the value of an individual query parameter, if available.
		/// </summary>
		/// <param name="QueryParameter">Query parameter name. This parameter is case insensitive.</param>
		/// <param name="Value">Query parameter value, if found.</param>
		/// <returns>If a query parameter with the given name was found.</returns>
		public bool TryGetQueryParameter(string QueryParameter, out string Value)
		{
			if (!(this.query is null))
				return this.query.TryGetValue(QueryParameter, out Value);
			else
			{
				Value = string.Empty;
				return false;
			}
		}

		/// <summary>
		/// Gets the value of an individual query parameter, or a default value, 
		/// if not found.
		/// </summary>
		/// <param name="QueryParameter">Query parameter name. This parameter is case insensitive.</param>
		/// <returns>Value of the query parameter if found; otherwise, the empty string.</returns>
		public string GetQueryParameter(string QueryParameter)
		{
			return this.GetQueryParameter(QueryParameter, string.Empty);
		}

		/// <summary>
		/// Gets the value of an individual query parameter, or a default value, 
		/// if not found.
		/// </summary>
		/// <param name="QueryParameter">Query parameter name. This parameter is case insensitive.</param>
		/// <param name="DefaultValue">Default value to return if the query parameter is not found.</param>
		/// <returns>Value of the query parameter if found; otherwise, the default value.</returns>
		public string GetQueryParameter(string QueryParameter, string DefaultValue)
		{
			if (this.TryGetQueryParameter(QueryParameter, out string Value))
				return Value;
			else
				return DefaultValue;
		}

		/// <summary>
		/// All query parameters.
		/// </summary>
		public KeyValuePair<string, string>[] QueryParameters => this.queryParameters;

		/// <summary>
		/// Parses a specific HTTP header field.
		/// </summary>
		/// <param name="KeyLower">Lower-case version of field name.</param>
		/// <param name="Key">Field name, as it appears in the header.</param>
		/// <param name="Value">Unparsed header field value</param>
		/// <returns>HTTP header field object, corresponding to the particular field.</returns>
		protected override HttpField ParseField(string KeyLower, string Key, string Value)
		{
			switch (KeyLower)
			{
				case "accept":
					if (this.accept is null)
						this.accept = new HttpFieldAccept(Key, Value);
					else
						this.accept.AppendRecords(Value);

					return this.accept;

				case "accept-encoding":
					if (this.acceptEncoding is null)
						this.acceptEncoding = new HttpFieldAcceptEncoding(Key, Value);
					else
						this.acceptEncoding.AppendRecords(Value);

					return this.acceptEncoding;

				case "accept-language":
					if (this.acceptLanguage is null)
						this.acceptLanguage = new HttpFieldAcceptLanguage(Key, Value);
					else
						this.acceptLanguage.AppendRecords(Value);

					return this.acceptLanguage;

				case "accept-charset": return this.acceptCharset = new HttpFieldAcceptCharset(Key, Value);
				case "authorization": return this.authorization = new HttpFieldAuthorization(Key, Value);
				case "cookie": return this.cookie = new HttpFieldCookie(Key, Value);
				case "expect": return this.expect = new HttpFieldExpect(Key, Value);
				case "from": return this.from = new HttpFieldFrom(Key, Value);
				case "host": return this.host = new HttpFieldHost(Key, Value);
				case "if-match": return this.ifMatch = new HttpFieldIfMatch(Key, Value);
				case "if-modified-since": return this.ifModifiedSince = new HttpFieldIfModifiedSince(Key, Value);
				case "if-none-match": return this.ifNoneMatch = new HttpFieldIfNoneMatch(Key, Value);
				case "if-range": return this.ifRange = new HttpFieldIfRange(Key, Value);
				case "if-unmodified-since": return this.ifUnmodifiedSince = new HttpFieldIfUnmodifiedSince(Key, Value);
				case "referer": return this.referer = new HttpFieldReferer(Key, Value);
				case "range": return this.range = new HttpFieldRange(Key, Value);
				case "user-agent": return this.userAgent = new HttpFieldUserAgent(Key, Value);
				case "upgrade-insecure-requests": return this.upgradeInsequreRequests = new HttpFieldUpgradeInsecureRequests(Key, Value);
				case "connection": return this.connection = new HttpField(Key, Value);
				case "upgrade": return this.upgrade = new HttpField(Key, Value);
				default: return base.ParseField(KeyLower, Key, Value);
			}
		}

		/// <summary>
		/// Accept HTTP Field header. (RFC 2616, §14.1)
		/// </summary>
		public HttpFieldAccept Accept
		{
			get => this.accept;
			internal set => this.accept = value;
		}

		/// <summary>
		/// Accept-Charset HTTP Field header. (RFC 2616, §14.2)
		/// </summary>
		public HttpFieldAcceptCharset AcceptCharset => this.acceptCharset;

		/// <summary>
		/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
		/// </summary>
		/// <remarks>Note: You can set property to null to disable Content Encoding.</remarks>
		public HttpFieldAcceptEncoding AcceptEncoding
		{
			get => this.acceptEncoding;
			set => this.acceptEncoding = value;
		}

		/// <summary>
		/// Accept-Language HTTP Field header. (RFC 2616, §14.4)
		/// </summary>
		public HttpFieldAcceptLanguage AcceptLanguage => this.acceptLanguage;

		/// <summary>
		/// Authorization HTTP Field header. (RFC 2616, §14.8)
		/// </summary>
		public HttpFieldAuthorization Authorization => this.authorization;

		/// <summary>
		/// Cookie HTTP Field header. (RFC 6265, §5.2)
		/// </summary>
		public HttpFieldCookie Cookie => this.cookie;

		/// <summary>
		/// Expect HTTP Field header. (RFC 2616, §14.20)
		/// </summary>
		public HttpFieldExpect Expect => this.expect;

		/// <summary>
		/// From HTTP Field header. (RFC 2616, §14.22)
		/// </summary>
		public HttpFieldFrom From => this.from;

		/// <summary>
		/// Host HTTP Field header. (RFC 2616, §14.23)
		/// </summary>
		public HttpFieldHost Host
		{
			get => this.host;
			internal set => this.host = value;
		}

		/// <summary>
		/// If-Match HTTP Field header. (RFC 2616, §14.24)
		/// </summary>
		public HttpFieldIfMatch IfMatch => this.ifMatch;

		/// <summary>
		/// If-Modified-Since HTTP Field header. (RFC 2616, §14.25)
		/// </summary>
		public HttpFieldIfModifiedSince IfModifiedSince => this.ifModifiedSince;

		/// <summary>
		/// If-None-Match HTTP Field header. (RFC 2616, §14.26)
		/// </summary>
		public HttpFieldIfNoneMatch IfNoneMatch => this.ifNoneMatch;

		/// <summary>
		/// If-Range HTTP Field header. (RFC 2616, §14.27)
		/// </summary>
		public HttpFieldIfRange IfRange => this.ifRange;

		/// <summary>
		/// If-Unmodified-Since HTTP Field header. (RFC 2616, §14.28)
		/// </summary>
		public HttpFieldIfUnmodifiedSince IfUnmodifiedSince => this.ifUnmodifiedSince;

		/// <summary>
		/// Referer HTTP Field header. (RFC 2616, §14.36)
		/// </summary>
		public HttpFieldReferer Referer => this.referer;

		/// <summary>
		/// Range HTTP Field header. (RFC 2616, §14.35)
		/// </summary>
		public HttpFieldRange Range => this.range;

		/// <summary>
		/// UserAgent HTTP Field header. (RFC 2616, §14.43)
		/// </summary>
		public HttpFieldUserAgent UserAgent => this.userAgent;

		/// <summary>
		/// Connection HTTP Field header. (RFC 2616, §14.10)
		/// </summary>
		public HttpField Connection => this.connection;

		/// <summary>
		/// Upgrade HTTP Field header. (RFC 2616, §14.42)
		/// </summary>
		public HttpField Upgrade => this.upgrade;

		/// <summary>
		/// Upgrade-Insecure-Requests HTTP Field header.
		/// https://www.w3.org/TR/upgrade-insecure-requests/
		/// </summary>
		public HttpFieldUpgradeInsecureRequests UpgradeInsecureRequests => this.upgradeInsequreRequests;

		/// <summary>
		/// If the method is safe, according to §9.1.1, RFC 2616, and the HTTP Method registry at IANA:
		/// http://www.iana.org/assignments/http-methods/http-methods.xhtml.
		/// </summary>
		public bool IsMethodSafe
		{
			get
			{
				switch (this.method)
				{
					case "GET":
					case "HEAD":
					case "OPTIONS":
					case "PRI":
					case "PROPFIND":
					case "REPORT":
					case "SEARCH":
					case "TRACE":
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// If the method is idempotent, according to §9.1.2, RFC 2616, and the HTTP Method registry at IANA:
		/// http://www.iana.org/assignments/http-methods/http-methods.xhtml.
		/// </summary>
		public bool IsMethodIdempotent
		{
			get
			{
				switch (this.method)
				{
					case "POST":
					case "CONNECT":
					case "LOCK":
					case "PATCH":
						return false;

					default:
						return true;
				}
			}
		}

		/// <summary>
		/// If the message contains, apart from the header, a message body also.
		/// </summary>
		public override bool HasMessageBody
		{
			get
			{
				// Summary taken from http://stackoverflow.com/questions/299628/is-an-entity-body-allowed-for-an-http-delete-request

				if (this.method == "TRACE")
					return false;

				if (!(this.expect is null) && this.expect.Continue100)
					return false;

				if (!(this.ContentLength is null) && this.ContentLength.ContentLength > 0)
					return true;

				if (!(this.TransferEncoding is null))
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets an absolute URL for the request.
		/// </summary>
		/// <returns>URL corresponding to request.</returns>
		public string GetURL()
		{
			return this.GetURL(true, false);
		}

		/// <summary>
		/// Gets an absolute URL for the request.
		/// </summary>
		/// <param name="IncludeQuery">If the query portion of the URL should be returned.</param>
		/// <returns>URL corresponding to request.</returns>
		public string GetURL(bool IncludeQuery)
		{
			return this.GetURL(IncludeQuery, false);
		}

		/// <summary>
		/// Gets an absolute URL for the request.
		/// </summary>
		/// <param name="IncludeQuery">If the query portion of the URL should be returned.</param>
		/// <param name="IncludeFragment">If the fragment portion of the URL should be returned.</param>
		/// <returns>URL corresponding to request.</returns>
		public string GetURL(bool IncludeQuery, bool IncludeFragment)
		{
			StringBuilder Result = new StringBuilder();

			Result.Append(this.uriScheme);
			Result.Append("://");
			Result.Append(this.host?.Value ?? string.Empty);
			Result.Append(this.resource);

			if (IncludeQuery && !string.IsNullOrEmpty(this.queryString))
			{
				Result.Append('?');
				Result.Append(this.queryString);
			}

			if (IncludeFragment && !string.IsNullOrEmpty(this.fragment))
			{
				Result.Append('#');
				Result.Append(this.fragment);
			}

			return Result.ToString();
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public bool IsAcceptable(string Alternative)
		{
			return this.accept?.IsAcceptable(Alternative) ?? true;
		}

		/// <summary>
		/// Checks if an alternative is acceptable to the client sending a request.
		/// </summary>
		/// <param name="Alternative">Alternative to check.</param>
		/// <returns>If content of the given type is acceptable to the client.</returns>
		public void AssertAcceptable(string Alternative)
		{
			if (!this.IsAcceptable(Alternative))
				throw new NotAcceptableException("Not acceptable response format: " + Alternative);
		}

		/// <summary>
		/// Gets the best alternative acceptable to the client.
		/// </summary>
		/// <param name="Alternatives">Array of alternatives to choose from.</param>
		/// <returns>The best choice. If none are acceptable, null is returned.</returns>
		public string IsAcceptable(string[] Alternatives)
		{
			return this.accept?.GetBestAlternative(Alternatives);
		}

	}
}
