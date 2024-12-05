using System.Collections.Generic;
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
		public const int DefaultHttp2InitialWindowSize = 65535;

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
		/// Default Enable Push Promises for an HTTP/2 connection (true).
		/// </summary>
		public const bool DefaultHttp2EnablePush = true;

		/// <summary>
		/// Default SETTINGS_NO_RFC7540_PRIORITIES is false, meaning priorities
		/// as defined in RFC7540 are used.
		/// </summary>
		public const bool DefaultNoRfc7540Priorities = false;

		private long connectionWindowSize;
		private long connectionBytesCommunicated = 0;
		private int headerTableSize;
		private int maxConcurrentStreams;
		private int initialWindowSize;
		private int maxFrameSize;
		private int maxHeaderListSize = HttpClientConnection.MaxHeaderSize;
		private bool enablePush;
		private bool noRfc7540Priorities;

		private readonly List<PendingWindowIncrement> pendingIncrements = new List<PendingWindowIncrement>();
		private bool hasPendingIncrements = false;

		/// <summary>
		/// HTTP/2 connection settings (SETTINGS).
		/// </summary>
		public ConnectionSettings()
		{
			this.connectionWindowSize = DefaultHttp2InitialWindowSize;
			this.initialWindowSize = DefaultHttp2InitialWindowSize;
			this.maxFrameSize = DefaultHttp2MaxFrameSize;
			this.maxConcurrentStreams = DefaultHttp2MaxConcurrentStreams;
			this.headerTableSize = DefaultHttp2HeaderTableSize;
			this.enablePush = DefaultHttp2EnablePush;
			this.noRfc7540Priorities = DefaultNoRfc7540Priorities;
		}

		/// <summary>
		/// HTTP/2 connection settings (SETTINGS).
		/// </summary>
		/// <param name="InitialWindowSize">Initial window size.</param>
		/// <param name="MaxFrameSize">Maximum frame size.</param>
		/// <param name="MaxConcurrentStreams">Maximum number of concurrent streams.</param>
		/// <param name="HeaderTableSize">Header table size.</param>
		/// <param name="EnablePush">If push promises are enabled.</param>
		/// <param name="NoRfc7540Priorities">If RFC7540 priorities are obsoleted.</param>
		public ConnectionSettings(int InitialWindowSize, int MaxFrameSize,
			int MaxConcurrentStreams, int HeaderTableSize, bool EnablePush,
			bool NoRfc7540Priorities)
		{
			this.connectionWindowSize = InitialWindowSize;
			this.initialWindowSize = InitialWindowSize;
			this.maxFrameSize = MaxFrameSize;
			this.maxConcurrentStreams = MaxConcurrentStreams;
			this.headerTableSize = HeaderTableSize;
			this.enablePush = EnablePush;
			this.noRfc7540Priorities |= NoRfc7540Priorities;
		}

		internal class PendingWindowIncrement
		{
			public Http2Stream Stream;
			public int NrBytes;
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
		public int HeaderTableSize => this.headerTableSize;

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
		public bool EnablePush => this.enablePush;

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
		public int MaxConcurrentStreams => this.maxConcurrentStreams;

		/// <summary>
		/// SETTINGS_INITIAL_WINDOW_SIZE (0x4):  Indicates the sender's initial
		/// window size(in octets) for stream-level flow control.The
		/// initial value is 2^16-1 (65,535) octets.
		/// 
		/// This setting affects the window size of all streams (see
		/// Section 6.9.2).
		/// 
		/// Values above the maximum flow-control window size of 2^31-1 MUST
		/// be treated as a connection error(Section 5.4.1) of type
		/// FLOW_CONTROL_ERROR.
		/// </summary>
		public int InitialWindowSize => this.initialWindowSize;

		/// <summary>
		/// SETTINGS_MAX_FRAME_SIZE (0x5):  Indicates the size of the largest
		/// frame payload that the sender is willing to receive, in octets.
		/// The initial value is 2^14 (16,384) octets.The value advertised
		/// by an endpoint MUST be between this initial value and the maximum
		/// allowed frame size (2^24-1 or 16,777,215 octets), inclusive.
		/// Values outside this range MUST be treated as a connection error
		/// (Section 5.4.1) of type PROTOCOL_ERROR.
		/// </summary>
		public int MaxFrameSize => this.maxFrameSize;

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
		public int MaxHeaderListSize => this.maxHeaderListSize;

		/// <summary>
		/// If RFC 7540 priorities are obsoleted, as defined in RFC 9218:
		/// https://www.rfc-editor.org/rfc/rfc9218.html
		/// </summary>
		public bool NoRfc7540Priorities => this.noRfc7540Priorities;

		/// <summary>
		/// Initialization step.
		/// </summary>
		internal int InitStep = 0;

		/// <summary>
		/// If the settings have been acknowledged or sent.
		/// </summary>
		internal bool AcknowledgedOrSent = false;

		/// <summary>
		/// If the connection has pending window size increments.
		/// </summary>
		internal bool HasPendingIncrements => this.hasPendingIncrements;

		/// <summary>
		/// Gets available pending window size increments.
		/// </summary>
		/// <returns>Array of increments.</returns>
		internal PendingWindowIncrement[] GetPendingIncrements()
		{
			PendingWindowIncrement[] Result = this.pendingIncrements.ToArray();
			this.pendingIncrements.Clear();
			this.hasPendingIncrements = false;
			return Result;
		}

		/// <summary>
		/// Adds a pending window size increment
		/// </summary>
		/// <param name="Stream">Stream</param>
		/// <param name="NrBytes">Size of increment, in number of bytes.</param>
		internal void AddPendingIncrement(Http2Stream Stream, int NrBytes)
		{
			int StreamId = Stream.StreamId;

			foreach (PendingWindowIncrement Increment in this.pendingIncrements)
			{
				if (Increment.Stream.StreamId == StreamId)
				{
					Increment.NrBytes += NrBytes;
					return;
				}
			}

			this.pendingIncrements.Add(new PendingWindowIncrement()
			{
				Stream = Stream,
				NrBytes = NrBytes
			});

			this.hasPendingIncrements = true;
		}

		/// <summary>
		/// Tries to parse a SETTINGS fame.
		/// 
		/// Ref: §6.5.2, RFC 7540
		/// https://datatracker.ietf.org/doc/html/rfc7540#section-6.5.2
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Settings">Settings object, if successful.</param>
		/// <returns>If able to parse the settings frame.</returns>
		public static bool TryParse(byte[] Data, out ConnectionSettings Settings)
		{
			return TryParse(Data, 0, Data.Length, out Settings);
		}
		
		/// <summary>
		/// Tries to parse a SETTINGS fame.
		/// 
		/// Ref: §6.5.2, RFC 7540
		/// https://datatracker.ietf.org/doc/html/rfc7540#section-6.5.2
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Frame starts at this offset.</param>
		/// <param name="Count">Number of bytes available.</param>
		/// <param name="Settings">Settings object, if successful.</param>
		/// <returns>If able to parse the settings frame.</returns>
		public static bool TryParse(byte[] Data, int Offset, int Count, out ConnectionSettings Settings)
		{
			BinaryReader Reader = new BinaryReader(Data, Offset, Count);
			return TryParse(Reader, null, out Settings) is null;
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
		public static Http2Error? TryParse(BinaryReader Reader, StringBuilder SnifferOutput, out ConnectionSettings Settings)
		{
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
						SnifferOutput?.AppendLine("RX: SETTINGS_HEADER_TABLE_SIZE = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.headerTableSize = (int)Value;
						break;

					case 2:
						SnifferOutput?.AppendLine("RX: SETTINGS_ENABLE_PUSH = " + Value.ToString());

						if (Value > 1)
							return Http2Error.ProtocolError;

						Settings.enablePush = Value != 0;
						break;

					case 3:
						SnifferOutput?.AppendLine("RX: SETTINGS_MAX_CONCURRENT_STREAMS = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.maxConcurrentStreams = (int)Value;
						break;

					case 4:
						SnifferOutput?.AppendLine("RX: SETTINGS_INITIAL_WINDOW_SIZE = " + Value.ToString());

						if (Value > 0x7fffffff)
							return Http2Error.FlowControlError;

						Settings.initialWindowSize = Value > int.MaxValue ? int.MaxValue : (int)Value;
						break;

					case 5:
						SnifferOutput?.AppendLine("RX: SETTINGS_MAX_FRAME_SIZE = " + Value.ToString());

						if (Value > 0x00ffffff)
							return Http2Error.ProtocolError;

						Settings.maxFrameSize = Value > int.MaxValue ? int.MaxValue : (int)Value;
						break;

					case 6:
						SnifferOutput?.AppendLine("RX: SETTINGS_MAX_HEADER_LIST_SIZE = " + Value.ToString());

						if (Value > int.MaxValue)
							return Http2Error.ProtocolError;

						Settings.maxHeaderListSize = (int)Value;
						break;

					case 9:
						SnifferOutput?.AppendLine("RX: SETTINGS_NO_RFC7540_PRIORITIES = " + Value.ToString());

						if (Value > 1)
							return Http2Error.ProtocolError;

						Settings.noRfc7540Priorities = Value != 0;
						break;

					default:
						SnifferOutput?.AppendLine("RX: (" + Key.ToString() + ", " + Value.ToString() + ")");

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
			BinaryWriter w = new BinaryWriter();

			w.WriteKeyValue(1, (uint)this.HeaderTableSize);
			w.WriteKeyValue(2, this.EnablePush ? 1U : 0U);
			w.WriteKeyValue(3, (uint)this.MaxConcurrentStreams);
			w.WriteKeyValue(4, (uint)this.InitialWindowSize);
			w.WriteKeyValue(5, (uint)this.MaxFrameSize);
			w.WriteKeyValue(6, (uint)this.MaxHeaderListSize);

			return w.ToArray();
		}

		/// <summary>
		/// Sets the window size increment for stream, modified using the WINDOW_UPDATE
		/// frame.
		/// </summary>
		/// <returns>If increment was valid.</returns>
		public bool SetWindowSizeIncrementLocked(int Increment)
		{
			if (Increment <= 0 || Increment > int.MaxValue - 1)
				return false;

			long Size = this.InitialWindowSize + Increment;

			if (Size < 0 || Size > int.MaxValue - 1)
				return false;

			this.connectionWindowSize = Size;

			return true;
		}

		/// <summary>
		/// Number of data bytes sent over the connection
		/// </summary>
		/// <param name="Increment">Increment</param>
		public void DataBytesSentLocked(int Increment)
		{
			this.connectionBytesCommunicated += Increment;
		}

		/// <summary>
		/// Connection window size
		/// </summary>
		public int ConnectionWindowSize => (int)(this.connectionWindowSize - this.connectionBytesCommunicated);
	}
}
