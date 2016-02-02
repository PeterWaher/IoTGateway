using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Waher.Content;

namespace Waher.Networking.HTTP.Test
{
	internal class CookieWebClient : WebClient
	{
		private CookieContainer cookies = new CookieContainer();
		private DateTime? ifModifiedSince = null;
		private DateTime? ifUnmodifiedSince = null;
		private string accept = null;

		public CookieWebClient()
			: base()
		{
		}

		public DateTime? IfModifiedSince
		{
			get { return this.ifModifiedSince; }
			set { this.ifModifiedSince = value; }
		}

		public DateTime? IfUnmodifiedSince
		{
			get { return this.ifUnmodifiedSince; }
			set { this.ifUnmodifiedSince = value; }
		}

		public string Accept
		{
			get { return this.accept; }
			set { this.accept = value; }
		}

		public CookieContainer Cookies
		{
			get { return this.cookies; }
			set { this.cookies = value; }
		}

		protected override WebRequest GetWebRequest(Uri Address)
		{
			WebRequest r = base.GetWebRequest(Address);

			HttpWebRequest request = r as HttpWebRequest;
			if (request != null)
			{
				request.CookieContainer = this.cookies;

				if (this.ifModifiedSince.HasValue)
					request.IfModifiedSince = this.ifModifiedSince.Value;

				if (this.ifUnmodifiedSince.HasValue)
					request.Headers.Set("If-Unmodified-Since", CommonTypes.EncodeRfc822(this.ifUnmodifiedSince.Value));

				if (!string.IsNullOrEmpty(this.accept))
					request.Accept = this.accept;
			}

			return r;
		}

		protected override WebResponse GetWebResponse(WebRequest Request, IAsyncResult ar)
		{
			WebResponse Response = base.GetWebResponse(Request, ar);
			this.CopyCookies(Response);
			return Response;
		}

		protected override WebResponse GetWebResponse(WebRequest Request)
		{
			WebResponse Response = base.GetWebResponse(Request);
			this.CopyCookies(Response);
			return Response;
		}

		private void CopyCookies(WebResponse Response)
		{
			HttpWebResponse WebResponse = Response as HttpWebResponse;
			if (WebResponse != null)
				cookies.Add(WebResponse.Cookies);
		}

	}
}
