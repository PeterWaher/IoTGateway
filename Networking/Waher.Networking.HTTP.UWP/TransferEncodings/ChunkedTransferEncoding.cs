using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Sniffers;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
	/// </summary>
	public class ChunkedTransferEncoding : TransferEncoding
	{
		private byte[] chunk = null;
		private int state = 0;
		private int chunkSize = 0;
		private int pos;

		/// <summary>
		/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ClientConnection">Client connection.</param>
		internal ChunkedTransferEncoding(Stream Output, HttpClientConnection ClientConnection)
			: base(Output, ClientConnection)
		{
		}

		/// <summary>
		/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ChunkSize">Chunk size.</param>
		/// <param name="ClientConnection">Client conncetion.</param>
		internal ChunkedTransferEncoding(Stream Output, int ChunkSize, HttpClientConnection ClientConnection)
			: base(Output, ClientConnection)
		{
			this.chunkSize = ChunkSize;
			this.chunk = new byte[ChunkSize];
			this.pos = 0;
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
			int i, j, c;
			byte b;

			for (i = Offset, c = Offset + NrRead; i < c; i++)
			{
				b = Data[i];
				switch (this.state)
				{
					case 0:     // Chunk size
						if (b >= '0' && b <= '9')
						{
							this.chunkSize <<= 4;
							this.chunkSize |= (b - '0');
						}
						else if (b >= 'A' && b <= 'F')
						{
							this.chunkSize <<= 4;
							this.chunkSize |= (b - 'A' + 10);
						}
						else if (b >= 'a' && b <= 'f')
						{
							this.chunkSize <<= 4;
							this.chunkSize |= (b - 'a' + 10);
						}
						else if (b == '\n')
						{
							if (this.chunkSize == 0)
							{
								NrAccepted = i - Offset + 1;
								return true;
							}
							else
								this.state = 4; // Receive data.
						}
						else if (b <= 32)
						{
							// Ignore whitespace.
						}
						else if (b == ';')
						{
							this.state++;   // Chunk extension.
						}
						else
						{
							NrAccepted = 0;
							this.invalidEncoding = true;
							return true;
						}
						break;

					case 1:     // Chunk extension
						if (b == '\n')
							this.state = 4; // Receive data.
						else if (b == '"')
							this.state++;
						break;

					case 2:     // Quoted string.
						if (b == '"')
							this.state--;
						else if (b == '\\')
							this.state++;
						break;

					case 3:     // Escape character
						this.state--;
						break;

					case 4:     // Data
						if (i + this.chunkSize <= c)
						{
							this.output.Write(Data, i, this.chunkSize);
							if (this.clientConnection != null)
								this.clientConnection.Server.DataTransmitted(this.chunkSize);
							i += this.chunkSize;
							this.chunkSize = 0;
							this.state++;
						}
						else
						{
							j = c - i;
							this.output.Write(Data, i, j);
							if (this.clientConnection != null)
								this.clientConnection.Server.DataTransmitted(j);
							this.chunkSize -= j;
							i = c;
						}
						break;

					case 5:     // Data CRLF
						if (b == '\n')
							this.state = 0;
						else if (b > 32)
						{
							NrAccepted = 0;
							this.invalidEncoding = true;
							return true;
						}
						break;
				}
			}

			NrAccepted = i - Offset;
			return false;
		}

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public override void Encode(byte[] Data, int Offset, int NrBytes)
		{
			int NrLeft;

			while (NrBytes > 0)
			{
				NrLeft = this.chunkSize - this.pos;

				if (NrLeft <= NrBytes)
				{
					Array.Copy(Data, Offset, this.chunk, this.pos, NrLeft);
					Offset += NrLeft;
					NrBytes -= NrLeft;
					this.pos += NrLeft;

					this.WriteChunk(true);
				}
				else
				{
					Array.Copy(Data, Offset, this.chunk, this.pos, NrBytes);
					this.pos += NrBytes;
					NrBytes = 0;
				}
			}
		}

		private void WriteChunk(bool Flush)
		{
			if (this.output != null)
			{
				string s = this.pos.ToString("X") + "\r\n";
				byte[] ChunkHeader = Encoding.ASCII.GetBytes(s);
				int Len = ChunkHeader.Length;
				byte[] Chunk = new byte[Len + this.pos + 2];

				Array.Copy(ChunkHeader, 0, Chunk, 0, Len);
				Array.Copy(this.chunk, 0, Chunk, Len, this.pos);
				Len += this.pos;
				Chunk[Len] = (byte)'\r';
				Chunk[Len + 1] = (byte)'\n';

				this.output.Write(Chunk, 0, Len + 2);

				if (Flush)
					this.output.Flush();

				if (this.clientConnection != null)
				{
					this.clientConnection.Server.DataTransmitted(Len + 2);
					this.clientConnection.TransmitBinary(Chunk);
				}
			}

			this.pos = 0;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override void Flush()
		{
			if (this.pos > 0)
				this.WriteChunk(true);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override void ContentSent()
		{
			if (this.output == null)
				return;

			if (this.pos > 0)
				this.WriteChunk(false);

			byte[] Chunk = new byte[5] { (byte)'0', 13, 10, 13, 10 };
			this.output.Write(Chunk, 0, 5);
			this.output.Flush();

			if (this.clientConnection != null)
			{
				this.clientConnection.Server.DataTransmitted(5);
				this.clientConnection.TransmitBinary(Chunk);
			}
		}

	}
}
