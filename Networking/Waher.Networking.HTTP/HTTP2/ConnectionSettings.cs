using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 connection settings (SETTINGS).
	/// </summary>
	public class ConnectionSettings
	{
		/// <summary>
		/// Default Initial Window Size for an HTTP/2 connection (65535).
		/// </summary>
		public const int DefaultHttp2InitialConnectionWindowSize = 65535;

		/// <summary>
		/// Default Maximum Frame Size for an HTTP/2 connection (4096).
		/// </summary>
		public const int DefaultHttp2MaxFrameSize = 16384;

		/// <summary>
		/// Default Maximum Concurrent Streams for an HTTP/2 connection (100).
		/// </summary>
		public const int DefaultHttp2MaxConcurrentStreams = 100;

		/// <summary>
		/// Default Header Table Size for an HTTP/2 connection (4096).
		/// </summary>
		public const int DefaultHttp2HeaderTableSize = 4096;

		/// <summary>
		/// Default Enable Push Promises for an HTTP/2 connection (false).
		/// </summary>
		/// <remarks>
		/// Notes on why Push is disabled by default in many HTTP/2 implementations:
		/// https://groups.google.com/a/chromium.org/g/blink-dev/c/K3rYLvmQUBY/m/vOWBKZGoAQAJ
		/// </remarks>
		public const bool DefaultHttp2EnablePush = false;

		/// <summary>
		/// Default Enable Connect Protocol over HTTP/2 (true).
		/// </summary>
		public const bool DefaultHttp2EnableConnectProtocol = true;

		/// <summary>
		/// Default SETTINGS_NO_RFC7540_PRIORITIES is false, meaning priorities
		/// as defined in RFC7540 are used.
		/// </summary>
		public const bool DefaultNoRfc7540Priorities = false;

		private int headerTableSize;
		private int maxConcurrentStreams;
		private int initialStreamWindowSize;
		private int maxFrameSize;
		private int maxHeaderListSize = HttpClientConnection.MaxHeaderSize;
		private bool enablePush;
		private bool noRfc7540Priorities;
		private bool enableConnectProtocol;

		/// <summary>
		/// HTTP/2 connection settings (SETTINGS).
		/// </summary>
		public ConnectionSettings()
		{
			this.initialStreamWindowSize = DefaultHttp2InitialConnectionWindowSize;
			this.maxFrameSize = DefaultHttp2MaxFrameSize;
			this.maxConcurrentStreams = DefaultHttp2MaxConcurrentStreams;
			this.headerTableSize = DefaultHttp2HeaderTableSize;
			this.enablePush = DefaultHttp2EnablePush;
			this.noRfc7540Priorities = DefaultNoRfc7540Priorities;
			this.enableConnectProtocol = DefaultHttp2EnableConnectProtocol;
		}

		/// <summary>
		/// HTTP/2 connection settings (SETTINGS).
		/// </summary>
		/// <param name="InitialStreamWindowSize">Initial stream window size.</param>
		/// <param name="MaxFrameSize">Maximum frame size.</param>
		/// <param name="MaxConcurrentStreams">Maximum number of concurrent streams.</param>
		/// <param name="HeaderTableSize">Header table size.</param>
		/// <param name="EnablePush">If push promises are enabled.</param>
		/// <param name="NoRfc7540Priorities">If RFC7540 priorities are obsoleted.</param>
		public ConnectionSettings(int InitialStreamWindowSize, int MaxFrameSize,
			int MaxConcurrentStreams, int HeaderTableSize, bool EnablePush,
			bool NoRfc7540Priorities)
		{
			this.initialStreamWindowSize = InitialStreamWindowSize;
			this.maxFrameSize = MaxFrameSize;
			this.maxConcurrentStreams = MaxConcurrentStreams;
			this.headerTableSize = HeaderTableSize;
			this.enablePush = EnablePush;
			this.noRfc7540Priorities |= NoRfc7540Priorities;
			this.enableConnectProtocol = DefaultHttp2EnableConnectProtocol;
		}

		/// <summary>
		/// SETTINGS_HEADER_TABLE_SIZE (0x1):  Allows the sender to inform the
		/// remote endpoint of the maximum size of the header compression
		/// table used to decode header blocks, in octets.The encoder can
		/// select any size equal to or less than this value by using
		/// signaling specific to the header compression format inside a
		/// 
		/// header block(see[COMPRESSION]).  The initial value is 4,096
		/// octets.
		/// </summary>
		public int HeaderTableSize
		{
			get => this.headerTableSize;
			internal set => this.headerTableSize = value;
		}


		/// <summary>
		/// SETTINGS_ENABLE_PUSH (0x2):  This setting can be used to disable
		/// server push(Section 8.2).  An endpoint MUST NOT send a
		/// 
		/// PUSH_PROMISE frame if it receives this parameter set to a value of
		/// 0.  An endpoint that has both set this parameter to 0 and had it
		/// acknowledged MUST treat the receipt of a PUSH_PROMISE frame as a
		/// connection error(Section 5.4.1) of type PROTOCOL_ERROR.
		/// 
		/// The initial value is 1, which indicates that server push is
		/// permitted.Any value other than 0 or 1 MUST be treated as a
		/// connection error(Section 5.4.1) of type PROTOCOL_ERROR.
		/// </summary>
		public bool EnablePush
		{
			get => this.enablePush;
			internal set => this.enablePush = value;
		}


		/// <summary>
		/// SETTINGS_MAX_CONCURRENT_STREAMS (0x3):  Indicates the maximum number
		/// of concurrent streams that the sender will allow.This limit is
		/// directional: it applies to the number of streams that the sender
		/// permits the receiver to create.Initially, there is no limit to
		/// this value.It is recommended that this value be no smaller than
		/// 100, so as to not unnecessarily limit parallelism.
		/// 
		/// A value of 0 for SETTINGS_MAX_CONCURRENT_STREAMS SHOULD NOT be
		/// treated as special by endpoints.A zero value does prevent the
		/// creation of new streams; however, this can also happen for any
		/// limit that is exhausted with active streams.Servers SHOULD only
		/// set a zero value for short durations; if a server does not wish to
		/// accept requests, closing the connection is more appropriate.
		/// </summary>
		public int MaxConcurrentStreams
		{
			get => this.maxConcurrentStreams;
			internal set => this.maxConcurrentStreams = value;
		}


		/// <summary>
		/// SETTINGS_INITIAL_WINDOW_SIZE (0x4):  Indicates the sender's initial
		/// window size (in octets) for stream-level flow control. The
		/// initial value is 2^16-1 (65,535) octets.
		/// 
		/// This setting affects the window size of all streams (see
		/// Section 6.9.2).
		/// 
		/// Values above the maximum flow-control window size of 2^31-1 MUST
		/// be treated as a connection error(Section 5.4.1) of type
		/// FLOW_CONTROL_ERROR.
		/// </summary>
		public int InitialStreamWindowSize
		{
			get => this.initialStreamWindowSize;
			internal set => this.initialStreamWindowSize = value;
		}

		/// <summary>
		/// SETTINGS_MAX_FRAME_SIZE (0x5):  Indicates the size of the largest
		/// frame payload that the sender is willing to receive, in octets.
		/// The initial value is 2^14 (16,384) octets.The value advertised
		/// by an endpoint MUST be between this initial value and the maximum
		/// allowed frame size (2^24-1 or 16,777,215 octets), inclusive.
		/// Values outside this range MUST be treated as a connection error
		/// (Section 5.4.1) of type PROTOCOL_ERROR.
		/// </summary>
		public int MaxFrameSize
		{
			get => this.maxFrameSize;
			internal set => this.maxFrameSize = value;
		}


		/// <summary>
		///  SETTINGS_MAX_HEADER_LIST_SIZE (0x6):  This advisory setting informs a
		///  peer of the maximum size of header list that the sender is
		///  prepared to accept, in octets.The value is based on the
		///  uncompressed size of header fields, including the length of the
		///  name and value in octets plus an overhead of 32 octets for each
		///  header field.
		///  
		///  For any given request, a lower limit than what is advertised MAY
		///  be enforced.The initial value of this setting is unlimited.
		/// </summary>
		public int MaxHeaderListSize
		{
			get => this.maxHeaderListSize;
			internal set => this.maxHeaderListSize = value;
		}

		/// <summary>
		/// If RFC 7540 priorities are obsoleted, as defined in RFC 9218:
		/// https://www.rfc-editor.org/rfc/rfc9218.html
		/// </summary>
		public bool NoRfc7540Priorities
		{
			get => this.noRfc7540Priorities;
			internal set => this.noRfc7540Priorities = value;
		}

		/// <summary>
		/// If the extended connect protocol is supported, as defined in RFC 8441:
		/// https://datatracker.ietf.org/doc/html/rfc8441
		/// </summary>
		public bool EnableConnectProtocol
		{
			get => this.enableConnectProtocol;
			internal set => this.enableConnectProtocol = value;
		}

		/// <summary>
		/// Initialization step.
		/// </summary>
		internal int InitStep = 0;

		/// <summary>
		/// If the settings have been acknowledged or sent.
		/// </summary>
		internal bool AcknowledgedOrSent = false;

		/// <summary>
		/// Tries to parse a SETTINGS fame.
		/// 
		/// Ref: §6.5.2, RFC 7540
		/// https://datatracker.ietf.org/doc/html/rfc7540#section-6.5.2
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Settings">Settings object, if successful.</param>
		/// <returns>If able to parse the settings frame.</returns>
		public static bool TryParse(bool ConstantBuffer, byte[] Data, ref ConnectionSettings Settings)
		{
			return TryParse(ConstantBuffer, Data, 0, Data.Length, ref Settings);
		}

		/// <summary>
		/// Tries to parse a SETTINGS fame.
		/// 
		/// Ref: §6.5.2, RFC 7540
		/// https://datatracker.ietf.org/doc/html/rfc7540#section-6.5.2
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Frame starts at this offset.</param>
		/// <param name="Count">Number of bytes available.</param>
		/// <param name="Settings">Settings object, if successful.</param>
		/// <returns>If able to parse the settings frame.</returns>
		public static bool TryParse(bool ConstantBuffer, byte[] Data, int Offset, int Count, ref ConnectionSettings Settings)
		{
			BinaryReader Reader = new BinaryReader(ConstantBuffer, Data, Offset, Count);
			return TryParse(Reader, null, ref Settings) is null;
		}

		/// <summary>
		/// Tries to parse a SETTINGS fame.
		/// 
		/// Ref: §6.5.2, RFC 7540
		/// https://datatracker.ietf.org/doc/html/rfc7540#section-6.5.2
		/// </summary>
		/// <param name="Reader">Binary data reader.</param>
		/// <param name="SnifferOutput">Optional sniffer output.</param>
		/// <param name="Settings">Settings object, if successful.</param>
		/// <returns>Null, if able to parse the settings frame, otherwise the corresponding error to return.</returns>
		public static Http2Error? TryParse(BinaryReader Reader, StringBuilder SnifferOutput, ref ConnectionSettings Settings)
		{
			if (Settings is null)
				Settings = new ConnectionSettings();

			while (Reader.HasMore)
			{
				if (Reader.BytesLeft < 6)
					return Http2Error.FrameSizeError;

				ushort Key = Reader.NextUInt16();
				uint Value = Reader.NextUInt32();

				switch (Key)
				{
					case 1:
						SnifferOutput?.AppendLine("SETTINGS_HEADER_TABLE_SIZE = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.headerTableSize = (int)Value;
						break;

					case 2:
						SnifferOutput?.AppendLine("SETTINGS_ENABLE_PUSH = " + Value.ToString());

						if (Value > 1)
							return Http2Error.ProtocolError;

						Settings.enablePush = Value != 0;
						break;

					case 3:
						SnifferOutput?.AppendLine("SETTINGS_MAX_CONCURRENT_STREAMS = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.maxConcurrentStreams = (int)Value;
						break;

					case 4:
						SnifferOutput?.AppendLine("SETTINGS_INITIAL_WINDOW_SIZE = " + Value.ToString());

						if (Value > 0x7fffffff)
							return Http2Error.FlowControlError;

						Settings.initialStreamWindowSize = Value > int.MaxValue ? int.MaxValue : (int)Value;
						break;

					case 5:
						SnifferOutput?.AppendLine("SETTINGS_MAX_FRAME_SIZE = " + Value.ToString());

						if (Value > 0x00ffffff)
							return Http2Error.ProtocolError;

						Settings.maxFrameSize = Value > int.MaxValue ? int.MaxValue : (int)Value;
						break;

					case 6:
						SnifferOutput?.AppendLine("SETTINGS_MAX_HEADER_LIST_SIZE = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.maxHeaderListSize = (int)Value;
						break;

					case 8:
						SnifferOutput?.AppendLine("SETTINGS_ENABLE_CONNECT_PROTOCOL = " + Value.ToString());

						if (Value > 1)
							return Http2Error.ProtocolError;

						Settings.enableConnectProtocol = Value != 0;
						break;

					case 9:
						SnifferOutput?.AppendLine("SETTINGS_NO_RFC7540_PRIORITIES = " + Value.ToString());

						if (Value > 1)
							return Http2Error.ProtocolError;

						Settings.noRfc7540Priorities = Value != 0;
						break;

					default:
						SnifferOutput?.AppendLine("(" + Key.ToString() + ", " + Value.ToString() + ")");

						break;  // Ignore
				}
			}

			return null;
		}

		/// <summary>
		/// Serializes current settings.
		/// </summary>
		/// <returns>Byte array</returns>
		public byte[] ToArray()
		{
			return this.ToArray(null);
		}

		/// <summary>
		/// Serializes current settings.
		/// </summary>
		/// <param name="SnifferOutput">Optional sniffer output.</param>
		/// <returns>Byte array</returns>
		public byte[] ToArray(StringBuilder SnifferOutput)
		{
			BinaryWriter w = new BinaryWriter();

			w.WriteKeyValue(1, (uint)this.HeaderTableSize);
			SnifferOutput?.AppendLine("SETTINGS_HEADER_TABLE_SIZE = " + this.HeaderTableSize.ToString());
			
			w.WriteKeyValue(2, this.EnablePush ? 1U : 0U);
			SnifferOutput?.AppendLine("SETTINGS_ENABLE_PUSH = " + (this.EnablePush ? 1 : 0).ToString());
			
			w.WriteKeyValue(3, (uint)this.MaxConcurrentStreams);
			SnifferOutput?.AppendLine("SETTINGS_MAX_CONCURRENT_STREAMS = " + this.MaxConcurrentStreams.ToString());
			
			w.WriteKeyValue(4, (uint)this.InitialStreamWindowSize);
			SnifferOutput?.AppendLine("SETTINGS_INITIAL_WINDOW_SIZE = " + this.InitialStreamWindowSize.ToString());
			
			w.WriteKeyValue(5, (uint)this.MaxFrameSize);
			SnifferOutput?.AppendLine("SETTINGS_MAX_FRAME_SIZE = " + this.MaxFrameSize.ToString());
			
			w.WriteKeyValue(6, (uint)this.MaxHeaderListSize);
			SnifferOutput?.AppendLine("SETTINGS_MAX_HEADER_LIST_SIZE = " + this.MaxHeaderListSize.ToString());

			if (this.noRfc7540Priorities)
			{
				w.WriteKeyValue(9, 1U);
				SnifferOutput?.AppendLine("SETTINGS_NO_RFC7540_PRIORITIES = 1");
			}

			w.WriteKeyValue(8, this.enableConnectProtocol ? 1U : 0U);
			SnifferOutput?.AppendLine("SETTINGS_ENABLE_CONNECT_PROTOCOL = " + (this.enableConnectProtocol ? 1 : 0).ToString());

			return w.ToArray();
		}

	}
}
