using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HTTP2;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Implements HTTP/2 transfer, in accordance with RFC 7540.
	/// </summary>
	public class Http2TransferEncoding : TransferEncoding
	{
		private readonly byte[] buffer;
		private readonly int bufferSize;
		private readonly Http2Stream stream;
		private Encoding dataEncoding = null;
		private long? contentLength;
		private long contentTransmitted = 0;
		private int pos = 0;
		private bool ended = false;

		/// <summary>
		/// Implements HTTP/2 transfer, in accordance with RFC 7540.
		/// </summary>
		/// <param name="Stream">HTTP/2 stream.</param>
		/// <param name="ContentLength">Content-Length, if known.</param>
		public Http2TransferEncoding(Http2Stream Stream, long? ContentLength)
			: base()
		{
			this.stream = Stream;
			this.contentLength = ContentLength;

			this.bufferSize = Stream.Connection.LocalSettings.MaxFrameSize;
			if (ContentLength.HasValue && ContentLength.Value < this.bufferSize)
				this.bufferSize = (int)ContentLength.Value;

			this.buffer = new byte[this.bufferSize];
		}

		/// <summary>
		/// If transmitted data is text.
		/// </summary>
		public Encoding DataEncoding
		{
			get => this.dataEncoding;
			internal set => this.dataEncoding = value;
		}

		/// <summary>
		/// Content-Length, if known.
		/// </summary>
		public long? ContentLength
		{
			get => this.contentLength;
			internal set => this.contentLength = value;
		}

		/// <summary>
		/// Is called when new binary data has been received that needs to be decoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrRead">Number of bytes read.</param>
		/// <returns>
		/// Bits 0-31: >Number of bytes accepted by the transfer encoding. If less than <paramref name="NrRead"/>, the rest is part of a separate message.
		/// Bit 32: If decoding has completed.
		/// Bit 33: If transmission to underlying stream failed.
		/// </returns>
		public override Task<ulong> DecodeAsync(byte[] Data, int Offset, int NrRead)
		{
			return Task.FromResult(0UL);    // TODO
		}

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public override async Task<bool> EncodeAsync(byte[] Data, int Offset, int NrBytes)
		{
			if (!this.ended)
			{
				this.contentTransmitted += NrBytes;
				this.ended = this.contentLength.HasValue && this.contentTransmitted >= this.contentLength.Value;

				while (NrBytes > 0)
				{
					int i = Math.Min(NrBytes, this.bufferSize - this.pos);
					Array.Copy(Data, Offset, this.buffer, this.pos, i);
					Offset += i;
					NrBytes -= i;
					this.pos += i;

					if (this.pos == this.bufferSize)
					{
						if (!await this.stream.WriteData(this.buffer, 0, this.pos, this.ended && NrBytes == 0, this.dataEncoding))
							return false;

						this.pos = 0;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override async Task<bool> FlushAsync()
		{
			if (this.pos > 0)
			{
				if (!await this.stream.WriteData(this.buffer, 0, this.pos, this.ended, this.dataEncoding))
					return false;

				this.pos = 0;
			}

			return true;
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override async Task<bool> ContentSentAsync()
		{
			if (!this.ended)
			{
				this.ended = true;

				if (!await this.stream.WriteData(this.buffer, 0, this.pos, this.ended, this.dataEncoding))
					return false;

				this.pos = 0;
			}

			return true;
		}
	}
}
