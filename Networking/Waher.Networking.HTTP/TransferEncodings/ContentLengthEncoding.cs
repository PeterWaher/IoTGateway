using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Sniffers;

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
		internal ContentLengthEncoding(Stream Output, long ContentLength, HttpClientConnection ClientConnection)
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
		/// <param name="NrAccepted">Number of bytes accepted by the transfer encoding. If less than <paramref name="NrRead"/>, the
		/// rest is part of a separate message.</param>
		/// <returns>If the encoding of the content is complete.</returns>
		public override bool Decode(byte[] Data, int Offset, int NrRead, out int NrAccepted)
		{
			if (NrRead > this.bytesLeft)
			{
				this.output.Write(Data, Offset, (int)this.bytesLeft);
				NrAccepted = (int)this.bytesLeft;
				this.bytesLeft = 0;
			}
			else
			{
				this.output.Write(Data, Offset, NrRead);
				NrAccepted = NrRead;
				this.bytesLeft -= NrRead;
			}

			return this.bytesLeft == 0;
		}

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public override void Encode(byte[] Data, int Offset, int NrBytes)
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
				this.output.Write(Data, Offset, (int)this.bytesLeft);
				NrBytes -= (int)this.bytesLeft;
				this.bytesLeft = 0;

				if (NrBytes > 0)
					throw new Exception("More bytes written than specified in Content-Length.");
			}
			else
			{
				this.output.Write(Data, Offset, NrBytes);
				this.bytesLeft -= NrBytes;
			}
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override void Flush()
		{
			this.output.Flush();
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override void ContentSent()
		{
			this.output.Flush();
		}
	}
}
