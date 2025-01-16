namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Frame Type
	/// 
	/// Ref: §6, RFC 7540
	/// https://datatracker.ietf.org/doc/html/rfc7540#section-6
	/// </summary>
	public enum FrameType : int
	{
		/// <summary>
		///  DATA frames (type=0x0) convey arbitrary, variable-length sequences of
		///  octets associated with a stream.
		/// </summary>
		Data = 0,

		/// <summary>
		/// The HEADERS frame (type=0x1) is used to open a stream (Section 5.1),
		/// and additionally carries a header block fragment. 
		/// </summary>
		Headers = 1,

		/// <summary>
		/// The PRIORITY frame (type=0x2) specifies the sender-advised priority
		/// of a stream (Section 5.3).
		/// </summary>
		Priority = 2,

		/// <summary>
		/// The RST_STREAM frame (type=0x3) allows for immediate termination of a
		/// stream.
		/// </summary>
		ResetStream = 3,

		/// <summary>
		/// The SETTINGS frame (type=0x4) conveys configuration parameters that
		/// affect how endpoints communicate, such as preferences and constraints
		/// on peer behavior.
		/// </summary>
		Settings = 4,

		/// <summary>
		/// The PUSH_PROMISE frame (type=0x5) is used to notify the peer endpoint
		/// in advance of streams the sender intends to initiate.
		/// </summary>
		PushPromise = 5,

		/// <summary>
		/// The PING frame (type=0x6) is a mechanism for measuring a minimal
		/// round-trip time from the sender, as well as determining whether an
		/// idle connection is still functional.
		/// </summary>
		Ping = 6,

		/// <summary>
		/// The GOAWAY frame (type=0x7) is used to initiate shutdown of a
		/// connection or to signal serious error conditions.
		/// </summary>
		GoAway = 7,

		/// <summary>
		/// The WINDOW_UPDATE frame (type=0x8) is used to implement flow control;
		/// see Section 5.2 for an overview.
		/// </summary>
		WindowUpdate = 8,

		/// <summary>
		/// The CONTINUATION frame (type=0x9) is used to continue a sequence of
		/// header block fragments (Section 4.3).
		/// </summary>
		Continuation = 9,

		/// <summary>
		/// PRIORITY_UPDATE, as defined in RFC 9218
		/// </summary>
		PriorityUpdate = 0x10
	}
}
