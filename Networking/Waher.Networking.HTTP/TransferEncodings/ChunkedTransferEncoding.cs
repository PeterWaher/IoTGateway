using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
	/// </summary>
	public class ChunkedTransferEncoding : TransferEncoding
	{
		private MemoryStream chunk = null;
		private int state = 0;
		private int chunkSize = 0;
		private int nrLeft;

		/// <summary>
		/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		public ChunkedTransferEncoding(Stream Output)
			: base(Output)
		{
		}

		/// <summary>
		/// Implements chunked transfer encoding, as defined in §3.6.1 RFC 2616.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		public ChunkedTransferEncoding(Stream Output, int ChunkSize)
			: base(Output)
		{
			this.chunkSize = ChunkSize;
			this.chunk = new MemoryStream(this.chunkSize);
			this.nrLeft = this.chunkSize;
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
					case 0:		// Chunk size
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
								this.state = 4;	// Receive data.
						}
						else if (b <= 32)
						{
							// Ignore whitespace.
						}
						else if (b == ';')
						{
							this.state++;	// Chunk extension.
						}
						else
						{
							NrAccepted = 0;
							this.invalidEncoding = true;
							return true;
						}
						break;

					case 1:		// Chunk extension
						if (b == '\n')
							this.state = 4;	// Receive data.
						else if (b == '"')
							this.state++;
						break;

					case 2:		// Quoted string.
						if (b == '"')
							this.state--;
						else if (b == '\\')
							this.state++;
						break;
						
					case 3:		// Escape character
						this.state--;
						break;

					case 4:		// Data
						if (i + this.chunkSize <= c)
						{
							this.output.Write(Data, i, this.chunkSize);
							i += this.chunkSize;
							this.chunkSize = 0;
							this.state++;
						}
						else
						{
							j = c - i;
							this.output.Write(Data, i, j);
							this.chunkSize -= j;
							i = c;
						}
						break;

					case 5:		// Data CRLF
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
			while (NrBytes > 0)
			{
				if (this.nrLeft <= NrBytes)
				{
					this.chunk.Write(Data, Offset, this.nrLeft);
					Offset += this.nrLeft;
					NrBytes -= this.nrLeft;

					this.WriteChunk();
				}
				else
				{
					this.chunk.Write(Data, Offset, NrBytes);
					this.nrLeft -= NrBytes;
				}
			}
		}

		private void WriteChunk()
		{
			string s = this.chunk.Position.ToString("X") + "\r\n";
			byte[] ChunkHeader = Encoding.ASCII.GetBytes(s);
			int l = ChunkHeader.Length;
			byte[] Chunk = new byte[l + this.chunkSize + 2];

			Array.Copy(ChunkHeader, 0, Chunk, 0, l);
			Array.Copy(this.chunk.GetBuffer(), 0, Chunk, l, this.chunkSize);
			l += this.chunkSize;
			Chunk[l] = (byte)'\r';
			Chunk[l + 1] = (byte)'\n';

			this.output.Write(Chunk, 0, l + 2);

			this.chunk.Position = 0;
			this.nrLeft = this.chunkSize;
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override void Flush()
		{
			this.WriteChunk();
		}

	}
}
