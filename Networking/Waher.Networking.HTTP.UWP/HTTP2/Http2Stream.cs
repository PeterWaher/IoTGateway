using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.WebSockets;
using Waher.Runtime.Temporary;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 stream
	/// </summary>
	public class Http2Stream
	{
		private readonly int streamId;
		private readonly HttpClientConnection connection;
		private readonly int headerInputWindowSize;
		private HttpRequestHeader headers = null;
		private MemoryStream buildingHeaders = null;
		private TemporaryStream inputDataStream = null;
		private StreamState state = StreamState.Idle;
		private WebSocket webSocket = null;
		private string protocol = null;
		private int rfc9218Priority = 3;
		private bool rfc9218Incremental = false;
		private bool upgradedToWebSocket = false;
		private int headerBytesReceived = 0;
		private long dataBytesReceived = 0;
		private long dataBytesTransmitted = 0;
		private long? contentLength = null;
		private int dataInputWindowSize;
		private bool regularHeaderFieldsReported = false;

		/// <summary>
		/// HTTP/2 stream, for testing purposes.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Settings">Settings.</param>
		internal Http2Stream(int StreamId, ConnectionSettings Settings)
		{
			this.streamId = StreamId;
			this.connection = null;
			this.dataInputWindowSize = Settings.InitialWindowSize;
			this.headerInputWindowSize = Settings.MaxHeaderListSize;
		}

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Connection">Connection</param>
		internal Http2Stream(int StreamId, HttpClientConnection Connection)
		{
			this.streamId = StreamId;
			this.connection = Connection;
			this.dataInputWindowSize = this.connection.LocalSettings.InitialWindowSize;
			this.headerInputWindowSize = Connection.LocalSettings.MaxHeaderListSize;
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public int StreamId => this.streamId;

		/// <summary>
		/// If headers are being built using multiple frames.
		/// </summary>
		public bool IsBuildingHeaders => !(this.buildingHeaders is null);

		/// <summary>
		/// Headers being built.
		/// </summary>
		public HttpRequestHeader Headers => this.headers;

		/// <summary>
		/// Header input window size.
		/// </summary>
		public int HeaderInputWindowSize => this.headerInputWindowSize;

		/// <summary>
		/// Header bytes received
		/// </summary>
		public int HeadersBytesReceived => this.headerBytesReceived;

		/// <summary>
		/// Data bytes received
		/// </summary>
		public long DataBytesReceived => this.dataBytesReceived;

		/// <summary>
		/// Data bytes transmitted
		/// </summary>
		public long DataBytesTransmitted => this.dataBytesTransmitted;

		/// <summary>
		/// Data Input window size
		/// </summary>
		public int DataInputWindowSize => this.dataInputWindowSize;

		/// <summary>
		/// Input data stream, if defined.
		/// </summary>
		public TemporaryStream InputDataStream => this.inputDataStream;

		/// <summary>
		/// HTTP/2 connection.
		/// </summary>
		internal HttpClientConnection Connection => this.connection;

		/// <summary>
		/// If the stream has been upgraded to a web-socket.
		/// </summary>
		public bool UpgradedToWebSocket => this.upgradedToWebSocket;

		/// <summary>
		/// Value of protocol pseudo-header.
		/// </summary>
		public string Protocol => this.protocol;

		/// <summary>
		/// Priority, as defined in RFC 9218
		/// </summary>
		public int Rfc9218Priority
		{
			get => this.rfc9218Priority;
			internal set => this.rfc9218Priority = value;
		}

		/// <summary>
		/// Incremental, as defined in RFC 9218
		/// </summary>
		public bool Rfc9218Incremental
		{
			get => this.rfc9218Incremental;
			internal set => this.rfc9218Incremental = value;
		}

		/// <summary>
		/// Stream state.
		/// </summary>
		public StreamState State
		{
			get => this.state;
			internal set => this.state = value;
		}

		/// <summary>
		/// Builds headers from multiple frames.
		/// </summary>
		/// <param name="Buffer">Buffer containing header payload.</param>
		/// <param name="Position">Position into buffer where header payload starts.</param>
		/// <param name="Count">Number of header bytes.</param>
		/// <returns>If successful adding headers to the stream.</returns>
		public bool BuildHeaders(byte[] Buffer, int Position, int Count)
		{
			this.headerBytesReceived += Count;
			if (this.headerBytesReceived < 0 || this.headerBytesReceived > this.headerInputWindowSize)
			{
				this.state = StreamState.Closed;
				return false;
			}
			else
			{
				if (this.buildingHeaders is null)
					this.buildingHeaders = new MemoryStream();

				this.buildingHeaders.Write(Buffer, Position, Count);
				return true;
			}
		}

		/// <summary>
		/// Finish building headers.
		/// </summary>
		/// <returns>Concatenation of headers fragments.</returns>
		public byte[] FinishBuildingHeaders()
		{
			byte[] Payload = this.buildingHeaders.ToArray();
			this.buildingHeaders.Dispose();
			this.buildingHeaders = null;
			return Payload;
		}

		/// <summary>
		/// Adds a parsed header.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Value">Value</param>
		/// <returns>Error message, if invalid.</returns>
		public string AddParsedHeader(string Name, string Value)
		{
			if (this.headers is null)
				this.headers = new HttpRequestHeader(2.0);

			if (string.IsNullOrEmpty(Name))
				return "Empty header.";

			bool IsPseudoHeader = Name[0] == ':';
			if (IsPseudoHeader)
			{
				if (this.regularHeaderFieldsReported)
					return "Pseudo-header fields cannot appear after regular header fields";
			}
			else
				this.regularHeaderFieldsReported = true;

			if (Name.ToLower() != Name)
				return "Header not in lower-case";

			switch (Name)
			{
				case ":method":
					if (!string.IsNullOrEmpty(this.headers.Method))
						return "Method defined multiple times.";

					this.headers.Method = Value;
					break;

				case ":authority":
					this.headers.AddField("host", Value, true);
					break;

				case ":path":
					if (string.IsNullOrEmpty(Value))
						return "Empty path.";

					if (!string.IsNullOrEmpty(this.headers.ResourcePart))
						return "Path defined multiple times.";

					this.headers.SetResource(Value, this.connection.Server.VanityResources);
					break;

				case ":scheme":
					if (!string.IsNullOrEmpty(this.headers.UriScheme))
						return "Scheme defined multiple times.";

					this.headers.UriScheme = Value;
					break;

				case ":protocol":
					if (!string.IsNullOrEmpty(this.protocol))
						return "Protocol defined multiple times.";

					this.protocol = Value;
					break;

				case ":status":
					return "Status is a response header field.";

				case "priority":
					this.headers.AddField(Name, Value, true);

					foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Value))
					{
						switch (P.Key)
						{
							case "u":
								if (int.TryParse(P.Value, out int i) && i >= 0 && i <= 7)
									this.rfc9218Priority = i;
								break;

							case "i":
								this.rfc9218Incremental = true;
								break;
						}
					}
					break;

				case "connection":
				case "proxy-connection":
				case "keep-alive":
				case "transfer-encoding":
				case "upgrade":
					return "Connection-specific header fields prohibited.";

				case "te":
					if (Value != "trailers")
						return "Invalid TE header field value: " + Value;

					this.headers.AddField(Name, Value, true);
					return null;

				default:
					if (IsPseudoHeader)
						return "Unknown pseudo-header: " + Name;

					this.headers.AddField(Name, Value, true);
					break;
			}

			return null;
		}

		/// <summary>
		/// Reports data payload received on stream.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Buffer containing data payload.</param>
		/// <param name="Position">Position into buffer where data payload starts.</param>
		/// <param name="Count">Number of data bytes.</param>
		/// <returns>If data was received ok (true), or if data was more than allowed and therefore
		/// rejected (false).</returns>
		public async Task<bool> DataReceived(bool ConstantBuffer, byte[] Buffer, int Position, int Count)
		{
			this.dataBytesReceived += Count;

			if (this.dataBytesReceived > this.dataInputWindowSize)
			{
				this.state = StreamState.Closed;
				return false;
			}
			else if (this.upgradedToWebSocket)
			{
				await this.webSocket.WebSocketDataReceived(ConstantBuffer, Buffer, Position, Count);
				// Ignore web-socket errors.
				return true;
			}
			else
			{
				if (this.inputDataStream is null)
					this.inputDataStream = new TemporaryStream();

				await this.inputDataStream.WriteAsync(Buffer, Position, Count);
				return true;
			}
		}

		/// <summary>
		/// Content-Length in <see cref="Headers"/>, if any.
		/// </summary>
		public long? ContentLength
		{
			get
			{
				if (!this.contentLength.HasValue &&
					!(this.headers?.ContentLength is null) &&
					long.TryParse(this.headers.ContentLength.Value, out long Length))
				{
					this.contentLength = Length;
				}

				return this.contentLength;
			}
		}

		/// <summary>
		/// Sets the output window size increment for stream, modified using the 
		/// WINDOW_UPDATE frame.
		/// </summary>
		public bool SetInputWindowSizeIncrement(uint Increment)
		{
			long Size = this.dataInputWindowSize + Increment;

			if (Size < 0 || Size > int.MaxValue - 1)
				return false;

			this.dataInputWindowSize = (int)Size;

			return true;
		}

		/// <summary>
		/// Writes HEADERS to the remote party.
		/// </summary>
		/// <param name="Headers">Binary headers</param>
		/// <param name="ExpectData">If data is expected.</param>
		/// <returns>If headers was written.</returns>
		public async Task<bool> WriteHeaders(byte[] Headers, bool ExpectData)
		{
			if (this.state == StreamState.Idle)
				this.state = StreamState.Open;

			int MaxFrameSize = this.connection.RemoteSettings.MaxFrameSize;
			byte Flags = (byte)(ExpectData ? 0 : 1);    // END_STREAM
			int Len = Headers.Length;

			if (Len <= MaxFrameSize)
			{
				Flags |= 4; // END_HEADERS

				return await this.connection.SendHttp2Frame(FrameType.Headers, Flags, false, this.streamId, this, Headers);
			}
			else
			{
				FrameType Type = FrameType.Headers;
				int Pos = 0;
				int Diff;

				while ((Diff = Len - Pos) > 0)
				{
					if (Diff > MaxFrameSize)
						Diff = MaxFrameSize;
					else
						Flags = 4; // END_HEADERS

					if (!await this.connection.SendHttp2Frame(Type, Flags, false, this.streamId, this, Headers, Pos, Diff, null))
						return false;

					Pos += Diff;
					Type = FrameType.Continuation;
					Flags = 0;
				}

				return true;
			}
		}

		/// <summary>
		/// Tries to write DATA to the remote party. Flow control might restrict the number
		/// of bytes written.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into buffer where data begins.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Last">If it is the last data to be written for this stream.</param>
		/// <param name="DataEncoding">Optional encoding, if data is text.</param>
		/// <returns>Number of bytes written. If negative, request failed.</returns>
		public async Task<int> TryWriteData(bool ConstantBuffer, byte[] Data, int Offset, int Count, bool Last,
			Encoding DataEncoding)
		{
			int NrBytes = await this.connection.TryWriteData(this, ConstantBuffer, Data, Offset, Count, Last, DataEncoding);
			if (NrBytes < 0)
				return -1;

			this.dataBytesTransmitted += NrBytes;

			if (Last && NrBytes == Count)
				this.state = StreamState.Closed;

			return NrBytes;
		}

		/// <summary>
		/// Tries to write all DATA to the remote party. Flow control might require multiple
		/// frames to be sent to send all data.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into buffer where data begins.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Last">If it is the last data to be written for this stream.</param>
		/// <param name="DataEncoding">Optional encoding, if data is text.</param>
		/// <returns>If successful in sending all data.</returns>
		public async Task<bool> TryWriteAllData(bool ConstantBuffer, byte[] Data, int Offset, int Count, bool Last,
			Encoding DataEncoding)
		{
			int NrWritten;

			while (Count > 0)
			{
				NrWritten = await this.TryWriteData(ConstantBuffer, Data, Offset, Count, Last, DataEncoding);
				if (NrWritten < 0)
					return false;

				Offset += NrWritten;
				Count -= NrWritten;
			}

			return true;
		}

		internal void Upgrade(WebSocket WebSocket)
		{
			this.webSocket = WebSocket;
			this.upgradedToWebSocket = true;
			this.state = StreamState.Open;
		}
	}
}
