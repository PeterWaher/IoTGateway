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
		private bool dataIsText = false;

		/// <summary>
		/// Implements HTTP/2 transfer, in accordance with RFC 7540.
		/// </summary>
		/// <param name="Stream">HTTP/2 stream.</param>
		public Http2TransferEncoding(Http2Stream Stream)
			: base()
		{
			this.stream = Stream;
		}

		/// <summary>
		/// If transmitted data is text.
		/// </summary>
		public bool DataIsText
		{
			get => this.dataIsText;
			internal set => this.dataIsText = value;
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
		public override Task<bool> EncodeAsync(byte[] Data, int Offset, int NrBytes)
		{
			return Task.FromResult(false);    // TODO
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override Task<bool> FlushAsync()
		{
			return Task.FromResult(false);    // TODO
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> ContentSentAsync()
		{
			return Task.FromResult(false);    // TODO
		}
	}
}
