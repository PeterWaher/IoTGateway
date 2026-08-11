using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// JSON-RPC request over internal transport.
	/// </summary>
	public class InternalJsonRpcCall : IJsonRpcCall
	{
		private readonly EventHandlerAsync<NotificationEventArgs> onEvent;
		private readonly ICommunicationLayer server;
		private readonly string baseUrl;
		private IUser user;
		private string? response = null;
		private string sessionId;

		/// <summary>
		/// JSON-RPC request over internal transport.
		/// </summary>
		/// <param name="Server">Server managing calls.</param>
		/// <param name="User">Authenticated user making the call.</param>
		/// <param name="BaseUrl">Base URL of web service.</param>
		/// <param name="OnEvent">Event handler called when asynchromous events are
		/// generated.</param>
		public InternalJsonRpcCall(ICommunicationLayer Server, IUser User, string BaseUrl,
			EventHandlerAsync<NotificationEventArgs> OnEvent)
			: this(Server, User, BaseUrl, OnEvent, "Internal")
		{
		}

		/// <summary>
		/// JSON-RPC request over internal transport.
		/// </summary>
		/// <param name="Server">Server managing calls.</param>
		/// <param name="User">Authenticated user making the call.</param>
		/// <param name="BaseUrl">Base URL of web service.</param>
		/// <param name="OnEvent">Event handler called when asynchromous events are
		/// generated.</param>
		/// <param name="Name">Name of the internal process.</param>
		public InternalJsonRpcCall(ICommunicationLayer Server, IUser User, string BaseUrl,
			EventHandlerAsync<NotificationEventArgs> OnEvent, string Name)
		{
			this.RemoteEndPoint = Name;
			this.sessionId = Name;
			this.server = Server;
			this.user = User;
			this.baseUrl = BaseUrl;
			this.onEvent = OnEvent;
		}

		/// <summary>
		/// Server managing calls.
		/// </summary>
		public ICommunicationLayer Server => this.server;

		/// <summary>
		/// Remote endpoint of the request.
		/// </summary>
		public string RemoteEndPoint { get; }

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		public IUser User
		{
			get => this.user;
			set => this.user = value;
		}

		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		public bool Encrypted => false;

		/// <summary>
		/// Cipher strength
		/// </summary>
		public int CipherStrength => 0;

		/// <summary>
		/// If the response has been sent.
		/// </summary>
		public bool ResponseSent => !string.IsNullOrEmpty(this.response);

		/// <summary>
		/// JSON-RPC response.
		/// </summary>
		public string Response => this.response ?? string.Empty;

		/// <summary>
		/// Keeps the request alive, without timing out
		/// </summary>
		/// <returns>If request found among current requests.</returns>
		public bool Ping()
		{
			return true;
		}

		/// <summary>
		/// Gets the base URL for the service.
		/// </summary>
		/// <returns>Bare URL</returns>
		public string GetBaseUrl()
		{
			return this.baseUrl;
		}

		/// <summary>
		/// Tries to get the MCP Session ID from a request.
		/// </summary>
		/// <param name="SessionId">MCP Session ID, if found.</param>
		/// <returns>If the MCP Session ID was found.</returns>
		public bool TryGetSessionId([NotNullWhen(true)] out string? SessionId)
		{
			SessionId = this.sessionId;
			return true;
		}

		/// <summary>
		/// Sets the MCP Session ID for the request.
		/// </summary>
		/// <param name="SessionId">JSON-RPC Session ID</param>
		public void SetSessionId(string SessionId)
		{
			this.sessionId = SessionId;
		}

		/// <summary>
		/// Checks the authentication of the request, if not done already.
		/// </summary>
		/// <param name="Session">Sniffable session.</param>
		/// <param name="RequiresAuthentication">If authentication is required.</param>
		/// <param name="AuthenticationSchemes">Available authentication schemes to use.</param>
		/// <param name="RequiredPrivileges">Privileges required by the method.</param>
		/// <returns>If request is authenticated.</returns>
		public Task<bool> CheckAuthentication(ICommunicationLayer? Session,
			bool RequiresAuthentication, HttpAuthenticationScheme[]? AuthenticationSchemes,
			string[]? RequiredPrivileges)
		{
			return Task.FromResult(!(this.user is null));
		}

		/// <summary>
		/// Sends a JSON-RPC error response.
		/// </summary>
		/// <param name="Error">Error to return.</param>
		public Task SendResponse(Exception Error)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sends the response back to the client. If the resource is synchronous, there's no need to call this method. Only asynchronous
		/// resources need to call this method explicitly.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		public Task SendResponse(int StatusCode, string StatusMessage)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sends the response back to the client.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		/// <param name="Response">Response object.</param>
		public Task SendResponse(int StatusCode, string StatusMessage, object? Response)
		{
			this.response = JSON.Encode(Response, false);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sends an event.
		/// </summary>
		/// <param name="Event">Event to send.</param>
		public Task SendEvent(NotificationEventArgs Event)
		{
			return this.onEvent.Raise(this.server, Event);
		}

	}
}
