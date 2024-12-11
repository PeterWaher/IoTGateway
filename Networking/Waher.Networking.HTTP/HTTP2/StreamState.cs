namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 stream state.
	/// 
	/// Ref: §5.1, RFC 7540
	/// https://datatracker.ietf.org/doc/html/rfc7540#section-5.1
	/// </summary>
	public enum StreamState
	{
		/// <summary>
		/// All streams start in the "idle" state.
		/// </summary>
		Idle,

		/// <summary>
		/// A stream in the "reserved (local)" state is one that has been
		/// promised by sending a PUSH_PROMISE frame.  A PUSH_PROMISE frame
		/// reserves an idle stream by associating the stream with an open
		/// stream that was initiated by the remote peer (see Section 8.2).
		/// </summary>
		ReservedLocal,

		/// <summary>
		/// A stream in the "reserved (remote)" state has been reserved by a
		/// remote peer.
		/// </summary>
		ReservedRemote,

		/// <summary>
		/// A stream in the "open" state may be used by both peers to send
		/// frames of any type.  In this state, sending peers observe
		/// advertised stream-level flow-control limits (Section 5.2).
		/// </summary>
		Open,

		/// <summary>
		/// A stream that is "half-closed (remote)" is no longer being used by
		/// the peer to send frames.  In this state, an endpoint is no longer
		/// obligated to maintain a receiver flow-control window.
		/// </summary>
		HalfClosedRemote,

		/// <summary>
		/// A stream that is in the "half-closed (local)" state cannot be used
		/// for sending frames other than WINDOW_UPDATE, PRIORITY, and
		/// RST_STREAM.
		/// </summary>
		HalfClosedLocal,

		/// <summary>
		/// The "closed" state is the terminal state.
		/// </summary>
		Closed,

		/// <summary>
		/// Stream is upgraded to a Web Socket, using RFC 8441
		/// </summary>
		WebSocket
	}
}
