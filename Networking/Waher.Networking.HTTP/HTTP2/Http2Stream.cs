using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 stream
	/// </summary>
	internal class Http2Stream
	{
		private readonly int streamId;
		private readonly HttpClientConnection connection;
		private HttpRequestHeader headers = null;
		private MemoryStream buildingHeaders = null;
		private int bufferSize;
		private byte[] outputBuffer;

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Connection">Connection</param>
		public Http2Stream(int StreamId, HttpClientConnection Connection)
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


	}
}
