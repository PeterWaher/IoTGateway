using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Cache;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Web Service for working with short URLs.
	/// </summary>
	public class UrlShortener : HttpSynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		private static readonly Cache<string, UrlRecord> shortUrls = new Cache<string, UrlRecord>(int.MaxValue, TimeSpan.FromDays(1), TimeSpan.FromHours(1));

		/// <summary>
		/// Web Service, generating QR-codes based on URI input.
		/// </summary>
		public UrlShortener()
			: this("/US")
		{
		}

		/// <summary>
		/// Web Service, generating QR-codes based on URI input.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public UrlShortener(string ResourceName)
			: base(ResourceName)
		{
		}

		private class UrlRecord
		{
			public string Url;
			public bool OneTimeUse;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			string s = Request.SubPath;
			if (s.StartsWith("/"))
				s = s.Substring(1);

			if (string.IsNullOrEmpty(s))
			{
				await Response.SendResponse(new BadRequestException("No short URL provided."));
				return;
			}

			if (!(shortUrls?.TryGetValue(s, out UrlRecord Rec) ?? false))
			{
				await Response.SendResponse(new NotFoundException("Short URL not found."));
				return;
			}

			if (Rec.OneTimeUse)
				shortUrls?.Remove(s);

			if ((Request.Header.QueryParameters?.Length ?? 0) > 0)
			{
				StringBuilder sb = new StringBuilder(Rec.Url);
				bool First = Rec.Url.IndexOf('?') < 0;

				foreach (KeyValuePair<string, string> P in Request.Header.QueryParameters)
				{
					if (First)
					{
						sb.Append('?');
						First = false;
					}
					else
						sb.Append('&');

					sb.Append(System.Web.HttpUtility.UrlEncode(P.Key));
					sb.Append('=');
					sb.Append(System.Web.HttpUtility.UrlEncode(P.Value));
				}

				await Response.SendResponse(new MovedPermanentlyException(sb.ToString()));
			}
			else
				await Response.SendResponse(new MovedPermanentlyException(Rec.Url));
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError)
			{
				await Response.SendResponse(Content.Error);
				return;
			}

			if (!(Content.Decoded is Dictionary<string, object> Obj) ||
				!Obj.TryGetValue("Url", out object Obj2) ||
				!(Obj2 is string Url) ||
				!Obj.TryGetValue("OneTimeUse", out Obj2) ||
				!(Obj2 is bool OneTimeUse))
			{
				await Response.SendResponse(new BadRequestException("Invalid request."));
				return;
			}

			await Response.Return(GetShortUrl(Url, OneTimeUse));
		}

		/// <summary>
		/// Shortens a URL temporarily. Shortened URLs are available at most for 24 hours
		/// (if used) and 1h if not used, or until the next server restart, or until it
		/// is used once if <paramref name="OneTimeUse"/> is true.
		/// </summary>
		/// <param name="Url">URL</param>
		/// <param name="OneTimeUse">If the shortened URL can be used only once.</param>
		/// <returns>Shortened URL</returns>
		public static string GetShortUrl(string Url, bool OneTimeUse)
		{
			string Id;

			do
			{
				Id = Base64Url.Encode(Gateway.NextBytes(32));
			}
			while (shortUrls?.ContainsKey(Id) ?? false);

			shortUrls?.Add(Id, new UrlRecord()
			{
				Url = Url,
				OneTimeUse = OneTimeUse
			});

			return Gateway.GetUrl("/US/" + Id);
		}
	}
}
