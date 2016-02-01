using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Waher.Networking.HTTP.Test
{
	internal class CookieWebClient : WebClient
	{
		private CookieContainer cookies = new CookieContainer();

		public CookieWebClient()
			: base()
		{
		}

		public CookieContainer Cookies
		{
			get { return cookies; }
			set { cookies = value; }
		}

		protected override WebRequest GetWebRequest(Uri Address)
		{
			WebRequest r = base.GetWebRequest(Address);
			
			HttpWebRequest request = r as HttpWebRequest;
			if (request != null)
				request.CookieContainer = this.cookies;
			
			return r;
		}

		protected override WebResponse GetWebResponse(WebRequest Request, IAsyncResult ar)
		{
			WebResponse response = base.GetWebResponse(Request, ar);
			this.CopyCookies(response);
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest Request)
		{
			WebResponse response = base.GetWebResponse(Request);
			this.CopyCookies(response);
			return response;
		}

		private void CopyCookies(WebResponse Response)
		{
			HttpWebResponse WebResponse = Response as HttpWebResponse;
			if (WebResponse != null)
				cookies.Add(WebResponse.Cookies);
		}

	}
}
