using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Events;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Returns the UTC Date &amp; Time. Can be used by the client to calculate latency.
	/// </summary>
	public class Ping : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Returns the UTC Date &amp; Time. Can be used by the client to calculate latency.
		/// </summary>
		public Ping()
			: base("/Ping")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			string[] Alternatives = XmlCodec.XmlContentTypes.Join(JsonCodec.DefaultContentType);
			string Alternative = Request.Header.IsAcceptable(Alternatives);

			if (Alternative is null)
			{
				await Response.SendResponse(new NotAcceptableException());
				return;
			}
			else if (Alternative == JsonCodec.DefaultContentType)
			{
				Response.ContentType = JsonCodec.DefaultContentType;

				StringBuilder Json = new StringBuilder();

				Json.Append("{\"ServerTimeMs\":");
				Json.Append((long)(DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalMilliseconds + 0.5));
				Json.Append('}');

				await Response.Write(Json.ToString());
			}
			else
			{
				Response.ContentType = XmlCodec.DefaultContentType;

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<ServerTime>");
				Xml.Append(XML.Encode(DateTime.UtcNow));
				Xml.Append("</ServerTime>");

				await Response.Write(Xml.ToString());
			}
		}

	}
}
