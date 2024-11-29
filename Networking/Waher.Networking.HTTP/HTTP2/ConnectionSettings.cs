using System;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 connection settings (SETTINGS).
	/// </summary>
	public class ConnectionSettings
	{
		private int bufferSize = 65535;
		private byte[] outputBuffer;

		/// <summary>
		/// HTTP/2 connection settings (SETTINGS).
		/// </summary>
		public ConnectionSettings()
		{
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
		public uint HeaderTableSize = 4096;

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
		public bool EnablePush = true;

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
		public uint MaxConcurrentStreams = 100;

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
		public uint InitialWindowSize = 65535;

		/// <summary>
		/// SETTINGS_MAX_FRAME_SIZE (0x5):  Indicates the size of the largest
		/// frame payload that the sender is willing to receive, in octets.
		/// The initial value is 2^14 (16,384) octets.The value advertised
		/// by an endpoint MUST be between this initial value and the maximum
		/// allowed frame size (2^24-1 or 16,777,215 octets), inclusive.
		/// Values outside this range MUST be treated as a connection error
		/// (Section 5.4.1) of type PROTOCOL_ERROR.
		/// </summary>
		public uint MaxFrameSize = 16384;

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
		public uint MaxHeaderListSize = HttpClientConnection.MaxHeaderSize;

		/// <summary>
		/// Initialization step.
		/// </summary>
		internal int InitStep = 0;

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

			Settings = new ConnectionSettings();

			while (Reader.HasMore)
			{
				if (Reader.BytesLeft < 6)
					return false;

				ushort Key = Reader.NextUInt16();
				uint Value = Reader.NextUInt32();

				switch (Key)
				{
					case 1:
						Settings.HeaderTableSize = Value;
						break;

					case 2:
						if (Value > 1)
							return false;

						Settings.EnablePush = Value != 0;
						break;

					case 3:
						Settings.MaxConcurrentStreams = Value;
						break;

					case 4:
						if (Value > 0x7fffffff)
							return false;

						Settings.InitialWindowSize = Value;
						break;

					case 5:
						if (Value > 0x00ffffff)
							return false;

						Settings.MaxFrameSize = Value;
						break;

					case 6:
						Settings.MaxHeaderListSize = Value;
						break;

					default:
						break;  // Ignore
				}
			}

			return true;
		}

		/// <summary>
		/// Serializes current settings.
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			BinaryWriter w = new BinaryWriter();

			w.WriteKeyValue(1, this.HeaderTableSize);
			w.WriteKeyValue(2, this.EnablePush ? 1U : 0U);
			w.WriteKeyValue(3, this.MaxConcurrentStreams);
			w.WriteKeyValue(4, this.InitialWindowSize);
			w.WriteKeyValue(5, this.MaxFrameSize);
			w.WriteKeyValue(6, this.MaxHeaderListSize);

			return w.ToArray();
		}

		/// <summary>
		/// Sets the window size increment for stream, modified using the WINDOW_UPDATE
		/// frame.
		/// </summary>
		public bool SetWindowSizeIncrement(uint Increment)
		{
			long Size = this.InitialWindowSize + Increment;

			if (Size > int.MaxValue - 1)
				return false;

			this.bufferSize = (int)Size;

			if (!(this.outputBuffer is null) && this.outputBuffer.Length < this.bufferSize)
				Array.Resize(ref this.outputBuffer, this.bufferSize);

			return true;
		}

		/// <summary>
		/// Buffer size
		/// </summary>
		public int BufferSize => this.bufferSize;
	}
}
