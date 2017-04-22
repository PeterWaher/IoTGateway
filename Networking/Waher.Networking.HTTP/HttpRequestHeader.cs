using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains information about all fields in an HTTP request header.
	/// </summary>
	public class HttpRequestHeader : HttpHeader
	{
		private Dictionary<string, string> query = null;
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
		private string method = string.Empty;
		private string resource = string.Empty;
		private string queryString = string.Empty;
		private string fragment = string.Empty;
		private string uriScheme = string.Empty;
		private double httpVersion = -1;

		/// <summary>
		/// Contains information about all fields in an HTTP request header.
		/// </summary>
		/// <param name="Header">HTTP Header.</param>
		/// <param name="UriScheme">URI Scheme.</param>
		public HttpRequestHeader(string Header, string UriScheme)
			: base(Header)
		{
			this.uriScheme = UriScheme;
		}

		/// <summary>
		/// Contains information about all fields in an HTTP request header.
		/// </summary>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="Resource">Resource.</param>
		/// <param name="Version">HTTP Version.</param>
		/// <param name="Headers">HTTP Header fields.</param>
		public HttpRequestHeader(string Method, string Resource, string Version, string UriScheme, params KeyValuePair<string, string>[] Headers)
			: base(Method + " " + Resource + " HTTP/" + Version, Headers)
		{
			this.uriScheme = UriScheme;
		}

		/// <summary>
		/// Parses the first row of an HTTP header.
		/// </summary>
		/// <param name="Row">First row.</param>
		protected override void ParseFirstRow(string Row)
		{
			int i = Row.IndexOf(' ');
			if (i > 0)
			{
				this.method = Row.Substring(0, i).ToUpper();

				int j = Row.LastIndexOf(' ');
				if (j > 0 && j < Row.Length - 5 && Row.Substring(j + 1, 5) == "HTTP/")
				{
					this.resource = Row.Substring(i + 1, j - i - 1).Trim();
					if (CommonTypes.TryParse(Row.Substring(j + 6), out this.httpVersion))
					{
						i = this.resource.IndexOf('?');
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

							this.query = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

							foreach (string Part in this.queryString.Split('&'))
							{
								i = Part.IndexOf('=');
								if (i < 0)
									this.query[Part] = string.Empty;
								else
									this.query[Part.Substring(0, i)] = Part.Substring(i + 1);
							}
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
					else
						this.httpVersion = -1;
				}
			}
		}

		/// <summary>
		/// HTTP Method
		/// </summary>
		public string Method { get { return this.method; } }

		/// <summary>
		/// Resource
		/// </summary>
		public string Resource { get { return this.resource; } }

		/// <summary>
		/// HTTP Version.
		/// </summary>
		public double HttpVersion { get { return this.httpVersion; } }

		/// <summary>
		/// Query string. To get the values of individual query parameters, use the <see cref="TryGetQueryParameter"/> method.
		/// </summary>
		public string QueryString { get { return this.queryString; } }

		/// <summary>
		/// Fragment.
		/// </summary>
		public string Fragment { get { return this.fragment; } }

		/// <summary>
		/// URI scheme.
		/// </summary>
		public string UriScheme { get { return this.uriScheme; } }

		/// <summary>
		/// Tries to get the value of an individual query parameter, if available.
		/// </summary>
		/// <param name="QueryParameter">Query parameter name. This parameter is case insensitive.</param>
		/// <param name="Value">Query parameter value, if found.</param>
		/// <returns>If a query parameter with the given name was found.</returns>
		public bool TryGetQueryParameter(string QueryParameter, out string Value)
		{
			if (this.query != null)
				return this.query.TryGetValue(QueryParameter, out Value);
			else
			{
				Value = string.Empty;
				return false;
			}
		}

		/// <summary>
		/// All query parameters.
		/// </summary>
		public KeyValuePair<string, string>[] QueryParameters
		{
			get
			{
				if (this.queryParameters == null)
				{
					if (this.query == null)
						this.queryParameters = new KeyValuePair<string, string>[0];
					else
					{
						KeyValuePair<string, string>[] P = new KeyValuePair<string, string>[this.query.Count];
						int i = 0;

						foreach (KeyValuePair<string, string> P2 in this.query)
							P[i++] = P2;

						this.queryParameters = P;
					}
				}

				return this.queryParameters;
			}
		}

		private KeyValuePair<string, string>[] queryParameters = null;

		protected override HttpField ParseField(string KeyLower, string Key, string Value)
		{
			switch (KeyLower)
			{
				case "accept": return this.accept = new HttpFieldAccept(Key, Value);
				case "accept-charset": return this.acceptCharset = new HttpFieldAcceptCharset(Key, Value);
				case "accept-encoding": return this.acceptEncoding = new HttpFieldAcceptEncoding(Key, Value);
				case "accept-language": return this.acceptLanguage = new HttpFieldAcceptLanguage(Key, Value);
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
				default: return base.ParseField(KeyLower, Key, Value);
			}
		}

		/// <summary>
		/// Accept HTTP Field header. (RFC 2616, §14.1)
		/// </summary>
		public HttpFieldAccept Accept { get { return this.accept; } }

		/// <summary>
		/// Accept-Charset HTTP Field header. (RFC 2616, §14.2)
		/// </summary>
		public HttpFieldAcceptCharset AcceptCharset { get { return this.acceptCharset; } }

		/// <summary>
		/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
		/// </summary>
		public HttpFieldAcceptEncoding AcceptEncoding { get { return this.acceptEncoding; } }

		/// <summary>
		/// Accept-Language HTTP Field header. (RFC 2616, §14.4)
		/// </summary>
		public HttpFieldAcceptLanguage AcceptLanguage { get { return this.acceptLanguage; } }

		/// <summary>
		/// Authorization HTTP Field header. (RFC 2616, §14.8)
		/// </summary>
		public HttpFieldAuthorization Authorization { get { return this.authorization; } }

		/// <summary>
		/// Cookie HTTP Field header. (RFC 6265, §5.2)
		/// </summary>
		public HttpFieldCookie Cookie { get { return this.cookie; } }

		/// <summary>
		/// Expect HTTP Field header. (RFC 2616, §14.20)
		/// </summary>
		public HttpFieldExpect Expect { get { return this.expect; } }

		/// <summary>
		/// From HTTP Field header. (RFC 2616, §14.22)
		/// </summary>
		public HttpFieldFrom From { get { return this.from; } }

		/// <summary>
		/// Host HTTP Field header. (RFC 2616, §14.23)
		/// </summary>
		public HttpFieldHost Host { get { return this.host; } }

		/// <summary>
		/// If-Match HTTP Field header. (RFC 2616, §14.24)
		/// </summary>
		public HttpFieldIfMatch IfMatch { get { return this.ifMatch; } }

		/// <summary>
		/// If-Modified-Since HTTP Field header. (RFC 2616, §14.25)
		/// </summary>
		public HttpFieldIfModifiedSince IfModifiedSince { get { return this.ifModifiedSince; } }

		/// <summary>
		/// If-None-Match HTTP Field header. (RFC 2616, §14.26)
		/// </summary>
		public HttpFieldIfNoneMatch IfNoneMatch { get { return this.ifNoneMatch; } }

		/// <summary>
		/// If-Range HTTP Field header. (RFC 2616, §14.27)
		/// </summary>
		public HttpFieldIfRange IfRange { get { return this.ifRange; } }

		/// <summary>
		/// If-Unmodified-Since HTTP Field header. (RFC 2616, §14.28)
		/// </summary>
		public HttpFieldIfUnmodifiedSince IfUnmodifiedSince { get { return this.ifUnmodifiedSince; } }

		/// <summary>
		/// Referer HTTP Field header. (RFC 2616, §14.36)
		/// </summary>
		public HttpFieldReferer Referer { get { return this.referer; } }

		/// <summary>
		/// Range HTTP Field header. (RFC 2616, §14.35)
		/// </summary>
		public HttpFieldRange Range { get { return this.range; } }

		/// <summary>
		/// UserAgent HTTP Field header. (RFC 2616, §14.43)
		/// </summary>
		public HttpFieldUserAgent UserAgent { get { return this.userAgent; } }

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

				if (this.expect != null && this.expect.Continue100)
					return false;

				if (this.ContentLength != null && this.ContentLength.ContentLength > 0)
					return true;

				if (this.TransferEncoding != null)
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
			Result.Append(this.host.Value);
			Result.Append(this.resource);

			if (IncludeQuery && !string.IsNullOrEmpty(this.queryString))
			{
				Result.Append('?');
				Result.Append(this.queryString);
			}

			if (IncludeFragment && !string.IsNullOrEmpty(this.fragment))
			{
				Result.Append('?');
				Result.Append(this.fragment);
			}

			return Result.ToString();
		}

	}
}
