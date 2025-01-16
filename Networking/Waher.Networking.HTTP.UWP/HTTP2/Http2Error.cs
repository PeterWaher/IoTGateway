namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 error codes
	/// </summary>
	public enum Http2Error : int
	{
		/// <summary>
		/// NO_ERROR (0x0):  The associated condition is not a result of an
		/// error.  For example, a GOAWAY might include this code to indicate
		/// graceful shutdown of a connection.
		/// </summary>
		NoError = 0,

		/// <summary>
		/// PROTOCOL_ERROR (0x1):  The endpoint detected an unspecific protocol
		/// error.  This error is for use when a more specific error code is
		/// not available.
		/// </summary>
		ProtocolError = 1,

		/// <summary>
		/// INTERNAL_ERROR (0x2):  The endpoint encountered an unexpected
		/// internal error.
		/// </summary>
		InternalError = 2,

		/// <summary>
		/// FLOW_CONTROL_ERROR(0x3):  The endpoint detected that its peer
		/// violated the flow-control protocol.
		/// </summary>
		FlowControlError = 3,

		/// <summary>
		/// SETTINGS_TIMEOUT (0x4):  The endpoint sent a SETTINGS frame but did
		/// not receive a response in a timely manner.See Section 6.5.3
		/// ("Settings Synchronization").
		/// </summary>
		SettingsTimeout = 4,

		/// <summary>
		/// STREAM_CLOSED(0x5) :  The endpoint received a frame after a stream
		/// was half-closed.
		/// </summary>
		StreamClosed = 5,

		/// <summary>
		/// FRAME_SIZE_ERROR(0x6):  The endpoint received a frame with an
		/// invalid size.
		/// </summary>
		FrameSizeError = 6,

		/// <summary>
		/// REFUSED_STREAM(0x7):  The endpoint refused the stream prior to
		/// performing any application processing(see Section 8.1.4 for
		/// details).
		/// </summary>
		RefusedStream = 7,

		/// <summary>
		/// CANCEL(0x8) :  Used by the endpoint to indicate that the stream is no
		/// longer needed.
		/// </summary>
		Cancel = 8,

		/// <summary>
		/// COMPRESSION_ERROR(0x9):  The endpoint is unable to maintain the
		/// header compression context for the connection.
		/// </summary>
		CompressionError = 9,

		/// <summary>
		/// CONNECT_ERROR(0xa):  The connection established in response to a
		/// CONNECT request(Section 8.3) was reset or abnormally closed.
		/// </summary>
		ConnectError = 10,

		/// <summary>
		/// ENHANCE_YOUR_CALM(0xb):  The endpoint detected that its peer is
		/// exhibiting a behavior that might be generating excessive load.
		/// </summary>
		EnhanceYourCalm = 11,

		/// <summary>
		/// INADEQUATE_SECURITY(0xc):  The underlying transport has properties
		/// that do not meet minimum security requirements(see Section 9.2).
		/// </summary>
		InadequateSecurity = 12,

		/// <summary>
		/// HTTP_1_1_REQUIRED(0xd) :  The endpoint requires that HTTP/1.1 be used
		/// instead of HTTP/2.
		/// </summary>
		Http11Required = 13,
	}
}
