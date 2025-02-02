using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Encodes content using a content length.
	/// </summary>
	public class ContentLengthEncoding : TransferEncoding
	{
		private readonly Encoding textEncoding;
		private long bytesLeft;
		private bool txText;

		/// <summary>
		/// Encodes content using a content length.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ContentLength">Content Length</param>
		/// <param name="ClientConnection">Client connection.</param>
		/// <param name="TxText">If text is transmitted (true), or binary data (false).</param>
		/// <param name="TextEncoding">Text encoding used (if any).</param>
		internal ContentLengthEncoding(IBinaryTransmission Output, long ContentLength, HttpClientConnection ClientConnection,
			bool TxText, Encoding TextEncoding)
			: base(Output, ClientConnection)
		{
			this.bytesLeft = ContentLength;
			this.txText = TxText;
			this.textEncoding = TextEncoding;
		}

		/// <summary>
		/// Is called when new binary data has been received that needs to be decoded.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrRead">Number of bytes read.</param>
		/// <returns>
		/// Bits 0-31: >Number of bytes accepted by the transfer encoding. If less than <paramref name="NrRead"/>, the rest is part of a separate message.
		/// Bit 32: If decoding has completed.
		/// Bit 33: If transmission to underlying stream failed.
		/// </returns>
		public override async Task<ulong> DecodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
		{
			ulong NrAccepted;

			if (this.output is null || !await this.output.SendAsync(ConstantBuffer, Data, Offset, NrRead))
				this.transferError = true;

			if (NrRead >= this.bytesLeft)
			{
				NrAccepted = (uint)this.bytesLeft;
				this.bytesLeft = 0;
			}
			else
			{
				NrAccepted = (uint)NrRead;
				this.bytesLeft -= NrRead;
			}

			if (this.bytesLeft == 0)
				NrAccepted |= 0x100000000UL;

			return NrAccepted;
		}

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		/// <param name="LastData">If no more data is expected.</param>
		public override async Task<bool> EncodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrBytes, bool LastData)
		{
			if (!(this.clientConnection is null))
			{
				int c = (int)Math.Min(this.bytesLeft, NrBytes);

				if (this.clientConnection.HasSniffers)
				{
					if (this.txText && c < 1000)
						this.clientConnection.TransmitText(this.textEncoding.GetString(Data, Offset, c));
					else
					{
						this.clientConnection.TransmitBinary(ConstantBuffer, Data, Offset, c);
						this.txText = false;
					}
				}

				this.clientConnection.Server.DataTransmitted(c);
			}

			if (this.bytesLeft <= NrBytes)
			{
				if (this.output is null || !await this.output.SendAsync(ConstantBuffer, Data, Offset, (int)this.bytesLeft))
					return false;

				NrBytes -= (int)this.bytesLeft;
				this.bytesLeft = 0;

				if (NrBytes > 0)
					throw new Exception("More bytes written than specified in Content-Length.");
			}
			else
			{
				if (this.output is null || !await this.output.SendAsync(ConstantBuffer, Data, Offset, NrBytes))
					return false;

				this.bytesLeft -= NrBytes;
			}

			return true;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		/// <param name="EndOfData">If no more data is expected.</param>
		public override Task<bool> FlushAsync(bool EndOfData)
		{
			return this.output?.FlushAsync() ?? Task.FromResult(false);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> ContentSentAsync()
		{
			return this.output?.FlushAsync() ?? Task.FromResult(false);
		}
	}
}
