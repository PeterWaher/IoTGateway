using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Delegate for HTTP Request events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Request">HTTP request.</param>
	public delegate void HttpRequestEventHandle(object Sender, HttpRequest Request);

	/// <summary>
	/// HTTP resource implementing the WebSocket Protocol as defined in RFC 6455:
	/// https://tools.ietf.org/html/rfc6455
	/// </summary>
	public class WebSocketListener : HttpSynchronousResource, IHttpGetMethod, IDisposable
	{
		private readonly string[] subProtocols;
		private readonly int maximumTextPayloadSize;
		private readonly int maximumBinaryPayloadSize;
		private readonly bool userSessions;

		/// <summary>
		/// HTTP resource implementing the WebSocket Protocol as defined in RFC 6455:
		/// https://tools.ietf.org/html/rfc6455
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If user sessions should be used.</param>
		/// <param name="MaximumTextPayloadSize">Maximum number of bytes in a text payload.
		/// If a payload is larger, it will be treated as a binary payload by default.</param>
		/// <param name="MaximumBinaryPayloadSize">Maximum number of bytes in a binary payload.
		/// (Binary payloads larger than 64K will be processed in temporary files, and not internal
		/// memory.)</param>
		/// <param name="SubProtocols">Supported websocket subprotocols.</param>
		public WebSocketListener(string ResourceName, bool UserSessions,
			int MaximumTextPayloadSize, int MaximumBinaryPayloadSize, params string[] SubProtocols)
			: base(ResourceName)
		{
			this.subProtocols = SubProtocols;
			this.userSessions = UserSessions;
			this.maximumBinaryPayloadSize = MaximumBinaryPayloadSize;
			this.maximumTextPayloadSize = MaximumTextPayloadSize;

			int i, c = this.subProtocols.Length;

			for (i = 0; i < c; i++)
				this.subProtocols[i] = this.subProtocols[i].ToLower();
		}

		/// <summary>
		/// WebSocket subprotocols supported by resource.
		/// </summary>
		public string[] SubProtocols => this.subProtocols;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.userSessions;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Maximum text payload size.
		/// </summary>
		public int MaximumTextPayloadSize => this.maximumTextPayloadSize;

		/// <summary>
		/// Maximum binary payload size.
		/// </summary>
		public int MaximumBinaryPayloadSize => this.maximumBinaryPayloadSize;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetHeaderField("Upgrade", out HttpField Upgrade) ||
				Upgrade.Value != "websocket")
			{
				throw new UpgradeRequiredException("websocket");
			}

			string Challenge;
			string WebSocketProtocol = null;
			int? WebSocketVersion;
			int i;

			if (Request.Header.TryGetHeaderField("Sec-WebSocket-Key", out HttpField Field))
				Challenge = Field.Value;
			else
				throw new BadRequestException("Sec-WebSocket-Key header field missing.");

			if (Request.Header.TryGetHeaderField("Sec-WebSocket-Protocol", out Field))
			{
				string[] Options = Field.Value.Split(',');

				foreach (string Option in Options)
				{
					i = Array.IndexOf<string>(this.subProtocols, Option.Trim().ToLower());
					if (i >= 0)
					{
						WebSocketProtocol = this.subProtocols[i];
						break;
					}
				}

				if (WebSocketProtocol is null)
					throw new NotSupportedException();
			}

			if (Request.Header.TryGetHeaderField("Sec-WebSocket-Version", out Field) &&
				int.TryParse(Field.Value, out i))
			{
				if (i < 13)
					throw new PreconditionFailedException();

				WebSocketVersion = i;
			}
			else
				throw new BadRequestException("Sec-WebSocket-Version header field missing.");

			if (Request.Header.TryGetHeaderField("Sec-WebSocket-Extensions", out Field))
			{
				// TODO: §9.1
			}

			if (Request.clientConnection is null)
				throw new ForbiddenException("Invalid connection.");

			WebSocket Socket = new WebSocket(this, Request, Response);
			
			this.Accept?.Invoke(this, new WebSocketEventArgs(Socket));

			string ChallengeResponse = Convert.ToBase64String(Hashes.ComputeSHA1Hash(
				Encoding.UTF8.GetBytes(Challenge.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

			Response.StatusCode = 101;
			Response.StatusMessage = "Switching Protocols";
			Response.SetHeader("Upgrade", "websocket");
			Response.SetHeader("Connection", "Upgrade");
			Response.SetHeader("Sec-WebSocket-Accept", ChallengeResponse);

			if (!(WebSocketProtocol is null))
				Response.SetHeader("Sec-WebSocket-Protocol", WebSocketProtocol);

			Request.clientConnection.Upgrade(Socket);

			await Response.SendResponse();

			this.Connected?.Invoke(this, new WebSocketEventArgs(Socket));
		}

		/// <summary>
		/// Event raised before accepting an incoming websocket connection request.
		/// It can be used to check the origin of the request. If the connection should
		/// be rejected, throw an <see cref="HttpException"/> within the event handler.
		/// </summary>
		public event WebSocketEventHandler Accept = null;

		/// <summary>
		/// Event raised after connection has been updated to a websocket connection.
		/// </summary>
		public event WebSocketEventHandler Connected = null;
	}
}
