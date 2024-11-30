using System;
using System.IO;
using System.Threading.Tasks;
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
		private HttpRequestHeader headers = null;
		private MemoryStream buildingHeaders = null;
		private TemporaryStream dataStream = null;
		private StreamState state = StreamState.Idle;
		private int bufferSize;
		private byte[] outputBuffer;

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Connection">Connection</param>
		internal Http2Stream(int StreamId, HttpClientConnection Connection)
		{
			this.streamId = StreamId;
			this.connection = Connection;
			this.bufferSize = this.connection.Settings.BufferSize;
			this.outputBuffer = null;
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
		/// Data stream, if defined.
		/// </summary>
		public TemporaryStream DataStream => this.dataStream;

		/// <summary>
		/// HTTP/2 connection.
		/// </summary>
		internal HttpClientConnection Connection => this.connection;

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
		public void BuildHeaders(byte[] Buffer, int Position, int Count)
		{
			this.buildingHeaders ??= new MemoryStream();
			this.buildingHeaders.Write(Buffer, Position, Count);
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

			if (!string.IsNullOrEmpty(Name) && Name[0] == ':')
			{
				switch (Name)
				{
					case ":method":
						this.headers.Method = Value;
						break;

					case ":path":
						this.headers.SetResource(Value, this.connection.Server.VanityResources);
						break;

					case ":scheme":
						this.headers.UriScheme = Value;
						break;

					default:
						this.headers.AddField(Name, Value, true);
						break;
				}
			}
			else
				this.headers.AddField(Name, Value, true);
		}

		/// <summary>
		/// Reports data payload received on stream.
		/// </summary>
		/// <param name="Buffer">Buffer containing data payload.</param>
		/// <param name="Position">Position into buffer where data payload starts.</param>
		/// <param name="Count">Number of data bytes.</param>
		/// <returns></returns>
		public async Task DataReceived(byte[] Buffer, int Position, int Count)
		{
			this.dataStream ??= new TemporaryStream();
			await this.dataStream.WriteAsync(Buffer, Position, Count);
		}

		/// <summary>
		/// Sets the window size increment for stream, modified using the WINDOW_UPDATE
		/// frame.
		/// </summary>
		public bool SetWindowSizeIncrement(uint Increment)
		{
			long Size = this.connection.Settings.BufferSize;
			Size += Increment;

			if (Size > int.MaxValue - 1)
				return false;

			this.bufferSize = (int)Size;

			if (!(this.outputBuffer is null) && this.outputBuffer.Length < this.bufferSize)
				Array.Resize(ref this.outputBuffer, this.bufferSize);

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

			int MaxFrameSize = this.connection.Settings.MaxFrameSize;
			byte Flags = (byte)(ExpectData ? 0 : 1);    // END_STREAM
			int Len = Headers.Length;

			if (Len <= MaxFrameSize)
			{
				Flags |= 4; // END_HEADERS

				return await this.connection.SendHttp2Frame(FrameType.Headers, Flags,
					(uint)this.streamId, Headers);
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

					if (!await this.connection.SendHttp2Frame(Type, Flags,
						(uint)this.streamId, Headers, Pos, Diff))
					{
						return false;
					}

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
		/// <returns>If data was written.</returns>
		public async Task<bool> WriteData(byte[] Data, int Offset, int Count, bool Last)
		{
			int MaxFrameSize = this.connection.Settings.MaxFrameSize;
			byte Flags = 0;

			if (Count <= MaxFrameSize)
			{
				if (Last)
				{
					Flags = 1;  // END_STREAM
					this.state = StreamState.HalfClosedLocal;
				}

				if (!await this.connection.SendHttp2Frame(FrameType.Data, Flags,
					(uint)this.streamId, Data, Offset, Count))
				{
					return false;
				}
			}
			else
			{
				int Left = Count;
				int Delta;

				while (Left > 0)
				{
					if (Left > MaxFrameSize)
						Delta = MaxFrameSize;
					else
					{
						Delta = Left;

						if (Last)
						{
							Flags = 1; // END_STREAM
							this.state = StreamState.HalfClosedLocal;
						}
					}

					if (!await this.connection.SendHttp2Frame(FrameType.Data, Flags,
						(uint)this.streamId, Data, Offset, Delta))
					{
						return false;
					}

					Offset += Delta;
					Left -= Delta;
				}
			}
		
			return true;
		}

	}
}
