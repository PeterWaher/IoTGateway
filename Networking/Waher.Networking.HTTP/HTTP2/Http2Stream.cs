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
		private readonly long headerInputWindowSize;
		private HttpRequestHeader headers = null;
		private MemoryStream buildingHeaders = null;
		private TemporaryStream inputDataStream = null;
		private StreamState state = StreamState.Idle;
		private WebSocket webSocket = null;
		private int rfc9218Priority = 3;
		private bool rfc9218Incremental = false;
		private bool upgradedToWebSocket = false;
		private long headerBytesReceived = 0;
		private long dataBytesReceived = 0;
		private long dataInputWindowSize;

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
		public long HeaderInputWindowSize => this.headerInputWindowSize;

		/// <summary>
		/// Header bytes received
		/// </summary>
		public long HeadersBytesReceived => this.headerBytesReceived;

		/// <summary>
		/// Data bytes received
		/// </summary>
		public long DataBytesReceived => this.dataBytesReceived;

		/// <summary>
		/// Data Input window size
		/// </summary>
		public long DataInputWindowSize => this.dataInputWindowSize;

		/// <summary>
		/// Input data stream, if defined.
		/// </summary>
		public TemporaryStream InputDataStream => this.inputDataStream;

		/// <summary>
		/// HTTP/2 connection.
		/// </summary>
		internal HttpClientConnection Connection => this.connection;

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
			if (this.headerBytesReceived > this.headerInputWindowSize)
			{
				this.state = StreamState.Closed;
				return false;
			}
			else
			{
				this.buildingHeaders ??= new MemoryStream();
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
		public void AddParsedHeader(string Name, string Value)
		{
			this.headers ??= new HttpRequestHeader(2.0);

			switch (Name)
			{
				case ":method":
					this.headers.Method = Value;
					break;

				case ":authority":
					this.headers.AddField("host", Value, true);
					break;

				case ":path":
					this.headers.SetResource(Value, this.connection.Server.VanityResources);
					break;

				case ":scheme":
					this.headers.UriScheme = Value;
					break;

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

				default:
					this.headers.AddField(Name, Value, true);
					break;
			}
		}

		/// <summary>
		/// Reports data payload received on stream.
		/// </summary>
		/// <param name="Buffer">Buffer containing data payload.</param>
		/// <param name="Position">Position into buffer where data payload starts.</param>
		/// <param name="Count">Number of data bytes.</param>
		/// <returns>If successful adding headers to the stream.</returns>
		public async Task<bool> DataReceived(byte[] Buffer, int Position, int Count)
		{
			this.dataBytesReceived += Count;
			if (this.dataBytesReceived > this.dataInputWindowSize)
			{
				this.state = StreamState.Closed;
				return false;
			}
			else
			{
				this.inputDataStream ??= new TemporaryStream();
				await this.inputDataStream.WriteAsync(Buffer, Position, Count);
				return true;
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

				return await this.connection.SendHttp2Frame(FrameType.Headers, Flags, false, this, Headers);
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

					if (!await this.connection.SendHttp2Frame(Type, Flags, false, this, Headers, Pos, Diff, null))
						return false;

					Pos += Diff;
					Type = FrameType.Continuation;
					Flags = 0;
				}

				return true;
			}
		}

		/// <summary>
		/// Writes DATA to the remote party.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into buffer where data begins.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Last">If it is the last data to be written for this stream.</param>
		/// <param name="DataEncoding">Optional encoding, if data is text.</param>
		/// <returns>If data was written.</returns>
		public async Task<bool> WriteData(byte[] Data, int Offset, int Count, bool Last, 
			Encoding DataEncoding)
		{
			if (!await this.connection.WriteData(this, Data, Offset, Count, Last, DataEncoding))
				return false;

			if (Last)
				this.state = StreamState.HalfClosedLocal;

			return true;
		}

		internal void Upgrade(WebSocket WebSocket)
		{
			this.webSocket = WebSocket;
			this.upgradedToWebSocket = true;
		}
	}
}
