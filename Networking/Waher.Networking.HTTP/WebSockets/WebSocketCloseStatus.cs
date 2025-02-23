namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Close status codes.
	/// </summary>
	public enum WebSocketCloseStatus
	{
		/// <summary>
		/// 1000 indicates a normal closure, meaning that the purpose for
		/// which the connection was established has been fulfilled.
		/// </summary>
		Normal = 1000,

		/// <summary>
		/// 1001 indicates that an endpoint is "going away", such as a server
		/// going down or a browser having navigated away from a page.
		/// </summary>
		GoingAway = 1001,

		/// <summary>
		/// 1002 indicates that an endpoint is terminating the connection due
		/// to a protocol error.
		/// </summary>
		ProtocolError = 1002,

		/// <summary>
		/// 1003 indicates that an endpoint is terminating the connection
		/// because it has received a type of data it cannot accept (e.g., an
		/// endpoint that understands only text data MAY send this if it
		/// receives a binary message).
		/// </summary>
		NotAcceptable = 1003,

		/// <summary>
		/// 1007 indicates that an endpoint is terminating the connection
		/// because it has received data within a message that was not
		/// consistent with the type of the message (e.g., non-UTF-8 [RFC3629]
		/// data within a text message).
		/// </summary>
		NotConsistent = 1007,

		/// <summary>
		/// 1008 indicates that an endpoint is terminating the connection
		/// because it has received a message that violates its policy.  This
		/// is a generic status code that can be returned when there is no
		/// other more suitable status code (e.g., 1003 or 1009) or if there
		/// is a need to hide specific details about the policy.
		/// </summary>
		PolicyViolation = 1008,

		/// <summary>
		/// 1009 indicates that an endpoint is terminating the connection
		/// because it has received a message that is too big for it to
		/// process.
		/// </summary>
		TooBig = 1009,

		/// <summary>
		/// 1010 indicates that an endpoint (client) is terminating the
		/// connection because it has expected the server to negotiate one or
		/// more extension, but the server didn't return them in the response
		/// message of the WebSocket handshake.  The list of extensions that
		/// are needed SHOULD appear in the /reason/ part of the Close frame.
		/// Note that this status code is not used by the server, because it
		/// can fail the WebSocket handshake instead.
		/// </summary>
		MissingExtension = 1010,

		/// <summary>
		/// 1011 indicates that a server is terminating the connection because
		/// it encountered an unexpected condition that prevented it from
		/// fulfilling the request.
		/// </summary>
		UnexpectedCondition = 1011
	}
}
