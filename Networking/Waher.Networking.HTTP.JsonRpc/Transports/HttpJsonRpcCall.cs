using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// JSON-RPC request over HTTP transport.
	/// </summary>
	public class HttpJsonRpcCall : IJsonRpcCall
	{
		private static readonly JsonCodec jsonCodec = new JsonCodec();

		private readonly HttpRequest request;
		private readonly HttpResponse response;

		/// <summary>
		/// JSON-RPC request over HTTP transport.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		public HttpJsonRpcCall(HttpRequest Request, HttpResponse Response)
		{
			this.request = Request;
			this.response = Response;
		}

		/// <summary>
		/// Server managing calls.
		/// </summary>
		public ICommunicationLayer Server => this.request.Server;

		/// <summary>
		/// Remote endpoint of the request.
		/// </summary>
		public string RemoteEndPoint => this.request.RemoteEndPoint;

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		public IUser User
		{
			get => this.request.User;
			internal set => this.request.User = value;
		}

		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		public bool Encrypted => this.request.Encrypted;

		/// <summary>
		/// Cipher strength
		/// </summary>
		public int CipherStrength => this.request.CipherStrength;

		/// <summary>
		/// If the response has been sent.
		/// </summary>
		public bool ResponseSent => this.response.ResponseSent;

		/// <summary>
		/// Keeps the request alive, without timing out
		/// </summary>
		/// <returns>If request found among current requests.</returns>
		public bool Ping()
		{
			return this.request.Ping();
		}

		/// <summary>
		/// Gets the base URL for the service.
		/// </summary>
		/// <returns>Bare URL</returns>
		public string GetBaseUrl()
		{
			return this.request.Header.GetURL(false, false);
		}

		/// <summary>
		/// Tries to get the MCP Session ID from a request.
		/// </summary>
		/// <param name="SessionId">MCP Session ID, if found.</param>
		/// <returns>If the MCP Session ID was found.</returns>
		public bool TryGetSessionId([NotNullWhen(true)] out string? SessionId)
		{
			if (this.request.Header.TryGetHeaderField("MCP-Session-Id", out HttpField SessionHeader))
			{
				SessionId = SessionHeader.Value;
				return true;
			}
			else
			{
				SessionId = null;
				return false;
			}
		}

		/// <summary>
		/// Sets the MCP Session ID for the request.
		/// </summary>
		/// <param name="SessionId">JSON-RPC Session ID</param>
		public void SetSessionId(string SessionId)
		{
			this.response.SetHeader("MCP-Session-Id", SessionId);
		}

		/// <summary>
		/// Checks the authentication of the request, if not done already.
		/// </summary>
		/// <param name="Session">Sniffable session.</param>
		/// <param name="RequiresAuthentication">If authentication is required.</param>
		/// <param name="AuthenticationSchemes">Available authentication schemes to use.</param>
		/// <param name="RequiredPrivileges">Privileges required by the method.</param>
		/// <returns>If request is authenticated.</returns>
		public async Task<bool> CheckAuthentication(ICommunicationLayer? Session,
			bool RequiresAuthentication, HttpAuthenticationScheme[]? AuthenticationSchemes,
			string[]? RequiredPrivileges)
		{
			IUser? User = this.request.User;

			if (User is null)
			{
				if ((AuthenticationSchemes?.Length ?? 0) == 0)
				{
					if (RequiresAuthentication)
					{
						Session?.Error("Access denied. No authentication schemes available.");
						await this.SendResponse(new ForbiddenException());
					}

					return false;
				}

				foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes!)
				{
					if (Scheme.RequireEncryption &&
						(!this.Encrypted || this.CipherStrength < Scheme.MinStrength))
					{
						continue;
					}

					if (Scheme.UserSessions && this.request.Session is null)
						this.request.GetSessionFromCookie();

					User = await Scheme.IsAuthenticated(this.request);
					if (!(User is null))
					{
						this.User = User;
						break;
					}
				}

				if (User is null && RequiresAuthentication)
				{
					ChunkedList<string> Challenges = new ChunkedList<string>();

					foreach (HttpAuthenticationScheme Scheme in AuthenticationSchemes)
					{
						if (Scheme.RequireEncryption &&
							(!this.Encrypted || this.CipherStrength < Scheme.MinStrength))
						{
							continue;
						}

						foreach (string Challenge in Scheme.GetChallenges(this.request))
							Challenges.Add(Challenge);
					}

					await this.response.SendResponse(new UnauthorizedException(
						Challenges.ToArray()));

					Session?.Error("Access denied. Unauthorized.");
				
					return false;
				}
			}

			if (User is null && !RequiresAuthentication)
				return false;

			if (!(User is null) && !(RequiredPrivileges is null))
			{
				foreach (string Privilege in RequiredPrivileges)
				{
					if (!User.HasPrivilege(Privilege))
					{
						Session?.Error("Access denied. Missing privilege: " + Privilege);

						await this.SendResponse(ForbiddenException.AccessDenied(this.request,
							this.request.Resource.ResourceName, User.UserName, Privilege));
						
						return false;
					}
				}
			}

			return !(User is null);
		}

		/// <summary>
		/// Sends a JSON-RPC error response.
		/// </summary>
		/// <param name="Error">Error to return.</param>
		public Task SendResponse(Exception Error)
		{
			return this.response.SendResponse(Error);
		}

		/// <summary>
		/// Sends the response back to the client. If the resource is synchronous, there's no need to call this method. Only asynchronous
		/// resources need to call this method explicitly.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		public async Task SendResponse(int StatusCode, string StatusMessage)
		{
			this.response.StatusCode = StatusCode;
			this.response.StatusMessage = StatusMessage;

			await this.response.SendResponse();
		}

		/// <summary>
		/// Sends the response back to the client.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		/// <param name="Response">Response object.</param>
		public async Task SendResponse(int StatusCode, string StatusMessage, object? Response)
		{
			ContentResponse Encoded;

			if (this.request.Header.Accept.IsAcceptable(JsonCodec.JsonRpcContentType))
			{
				Encoded = await jsonCodec.EncodeAsync(Response, Encoding.UTF8, null, 
					JsonCodec.JsonRpcContentType);
			}
			else
			{
				string ContentType = this.request.Header.Accept.GetBestAlternative(
					JsonCodec.JsonContentTypes);

				Encoded = await jsonCodec.EncodeAsync(Response, Encoding.UTF8, null, 
					ContentType);
			}

			if (Encoded.HasError)
				await this.SendResponse(Encoded.Error);
			else
			{
				this.response.StatusCode = StatusCode;
				this.response.StatusMessage = StatusMessage;

				if (StatusCode != 204)
				{
					this.response.ContentType = Encoded.ContentType;

					await this.response.Write(true, Encoded.Encoded, 0, Encoded.Encoded.Length);
				}

				await this.response.SendResponse();
			}
		}

		/// <summary>
		/// Sends an event.
		/// </summary>
		/// <param name="Event">Event to send.</param>
		public async Task SendEvent(NotificationEventArgs Event)
		{
			IEnumerable<KeyValuePair<string, object>> Fields = Event.Fields;
			StringBuilder sb = new StringBuilder();
			string? Comment = Event.Comment;
			bool Empty = true;

			if (!string.IsNullOrEmpty(Comment))
			{
				Empty = false;
				sb.Append(Comment);
				if (Comment.IndexOfAny(CommonTypes.CRLF) >= 0)
				{
					foreach (string Line in Comment.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
					{
						sb.Append(": ");
						sb.Append(Line);
						sb.Append("\r\n");
					}
				}
				else
				{
					sb.Append(": ");
					sb.Append(Comment);
					sb.Append("\r\n");
				}
			}

			if (!(Fields is null))
			{
				foreach (KeyValuePair<string, object> P in Fields)
				{
					Empty = false;

					if (!(P.Value is string s))
						s = JSON.Encode(P.Value, false);

					if (s.IndexOfAny(CommonTypes.CRLF) >= 0)
					{
						foreach (string Line in s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
						{
							sb.Append(P.Key);
							sb.Append(": ");
							sb.Append(Line);
							sb.Append("\r\n");
						}
					}
					else
					{
						sb.Append(P.Key);
						sb.Append(": ");
						sb.Append(s);
						sb.Append("\r\n");
					}
				}
			}

			if (Empty)
				sb.Append(":\r\n");

			sb.Append("\r\n");

			await this.response.Write(sb.ToString());
			await this.response.Flush(false);
		}
	}
}
