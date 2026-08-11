using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// Interface for JSON-RPC requests.
	/// </summary>
	public interface IJsonRpcCall
	{
		/// <summary>
		/// Server managing calls.
		/// </summary>
		ICommunicationLayer Server { get; }

		/// <summary>
		/// Remote endpoint of the request.
		/// </summary>
		string RemoteEndPoint { get; }

		/// <summary>
		/// Authenticated user, if available, or null if not available.
		/// </summary>
		IUser User { get; }

		/// <summary>
		/// If the connection is encrypted or not.
		/// </summary>
		bool Encrypted { get; }

		/// <summary>
		/// Cipher strength
		/// </summary>
		int CipherStrength { get; }

		/// <summary>
		/// If the response has been sent.
		/// </summary>
		bool ResponseSent { get; }

		/// <summary>
		/// Keeps the request alive, without timing out
		/// </summary>
		/// <returns>If request found among current requests.</returns>
		bool Ping();

		/// <summary>
		/// Gets the base URL for the service.
		/// </summary>
		/// <returns>Bare URL</returns>
		string GetBaseUrl();

		/// <summary>
		/// Tries to get the MCP Session ID from a request.
		/// </summary>
		/// <param name="SessionId">MCP Session ID, if found.</param>
		/// <returns>If the MCP Session ID was found.</returns>
		bool TryGetSessionId([NotNullWhen(true)] out string? SessionId);

		/// <summary>
		/// Sets the JSON-RPC Session for the request.
		/// </summary>
		/// <param name="SessionId">Session ID</param>
		void SetSessionId(string SessionId);

		/// <summary>
		/// Checks the authentication of the request, if not done already.
		/// </summary>
		/// <param name="Session">Sniffable session.</param>
		/// <param name="RequiresAuthentication">If authentication is required.</param>
		/// <param name="AuthenticationSchemes">Available authentication schemes to use.</param>
		/// <param name="RequiredPrivileges">Privileges required by the method.</param>
		/// <returns>If request is authenticated.</returns>
		Task<bool> CheckAuthentication(ICommunicationLayer? Session, bool RequiresAuthentication,
			HttpAuthenticationScheme[]? AuthenticationSchemes, string[]? RequiredPrivileges);

		/// <summary>
		/// Sends a JSON-RPC error response.
		/// </summary>
		/// <param name="Error">Error to return.</param>
		Task SendResponse(Exception Error);

		/// <summary>
		/// Sends the response back to the client.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		Task SendResponse(int StatusCode, string StatusMessage);

		/// <summary>
		/// Sends the response back to the client.
		/// </summary>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		/// <param name="Response">Response object.</param>
		Task SendResponse(int StatusCode, string StatusMessage, object? Response);

		/// <summary>
		/// Sends an event.
		/// </summary>
		/// <param name="Event">Event to send.</param>
		Task SendEvent(NotificationEventArgs Event);
	}
}
