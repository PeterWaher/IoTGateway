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
		private readonly Http2Stream stream;
		private Encoding dataEncoding = null;
		private long? contentLength;
		private long contentTransmitted = 0;
		private bool ended = false;
		private bool dataIsText = false;

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
		}

		/// <summary>
		/// If transmitted data is text.
		/// </summary>
		public Encoding DataEncoding
		{
			get => this.dataEncoding;
			internal set
			{
				this.dataEncoding = value;
				this.dataIsText = !(value is null);
			}
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

				if (!await this.stream.WriteData(Data, Offset, NrBytes, this.ended))
					return false;

				if (this.dataIsText && this.stream.Connection.HasSniffers)
					await this.stream.Connection.TransmitText(this.dataEncoding.GetString(Data, Offset, NrBytes));
			}
			
			return true;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override Task<bool> FlushAsync()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> ContentSentAsync()
		{
			if (!this.ended)
			{
				this.ended = true;
				return this.stream.WriteData(Array.Empty<byte>(), 0, 0, this.ended);
			}
			else
				return Task.FromResult(true);
		}
	}
}
