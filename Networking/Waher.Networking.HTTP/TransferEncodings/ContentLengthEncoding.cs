using System;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Encodes content using a content length.
	/// </summary>
	public class ContentLengthEncoding : TransferEncoding
	{
		private long bytesLeft;

		/// <summary>
		/// Encodes content using a content length.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ContentLength">Content Length</param>
		/// <param name="ClientConnection">Client connection.</param>
		internal ContentLengthEncoding(IBinaryTransmission Output, long ContentLength, HttpClientConnection ClientConnection)
			: base(Output, ClientConnection)
		{
			this.bytesLeft = ContentLength;
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
		public override async Task<ulong> DecodeAsync(byte[] Data, int Offset, int NrRead)
		{
			ulong NrAccepted;
			if (!(this.output is null) && !await this.output.SendAsync(Data, Offset, (int)this.bytesLeft))
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
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public override async Task<bool> EncodeAsync(byte[] Data, int Offset, int NrBytes)
		{
			if (this.clientConnection != null)
			{
				int c = (int)Math.Min(this.bytesLeft, NrBytes);

				if (Offset == 0 && c == Data.Length)
					this.clientConnection.TransmitBinary(Data);
				else
				{
					byte[] Data2 = new byte[c];
					Array.Copy(Data, Offset, Data2, 0, c);
					this.clientConnection.TransmitBinary(Data2);
				}

				this.clientConnection.Server.DataTransmitted(c);
			}

			if (this.bytesLeft <= NrBytes)
			{
				if (!(this.output is null) && !await this.output.SendAsync(Data, Offset, (int)this.bytesLeft))
					return false;

				NrBytes -= (int)this.bytesLeft;
				this.bytesLeft = 0;

				if (NrBytes > 0)
					throw new Exception("More bytes written than specified in Content-Length.");
			}
			else
			{
				if (!(this.output is null) && !await this.output.SendAsync(Data, Offset, NrBytes))
					return false;

				this.bytesLeft -= NrBytes;
			}

			return true;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override Task<bool> FlushAsync()
		{
			return this.output.FlushAsync();
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> ContentSentAsync()
		{
			return this.output.FlushAsync();
		}
	}
}
