using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Class returning a precompressed file.
	/// </summary>
	public class PrecompressedFileReturner : TransferEncoding
	{
		private readonly FileInfo precompressedFile;
		private TransferEncoding uncompressedStream;
		private bool finished = false;

		/// <summary>
		/// Class returning a precompressed file.
		/// </summary>
		/// <param name="PrecompressedFile">Reference to precompressed file.</param>
		/// <param name="UncompressedStream">Output stream.</param>
		public PrecompressedFileReturner(FileInfo PrecompressedFile, TransferEncoding UncompressedStream)
			: base(null, UncompressedStream)
		{
			this.precompressedFile = PrecompressedFile;
			this.uncompressedStream = UncompressedStream;
		}

		/// <summary>
		/// Is called when the header is complete, and before content is being transferred.
		/// </summary>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="ExpectContent">If content is expected.</param>
		public override Task BeforeContentAsync(HttpResponse Response, bool ExpectContent)
		{
			return this.uncompressedStream.BeforeContentAsync(Response, ExpectContent);
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
		public override Task<ulong> DecodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
		{
			return Task.FromResult(0x100000000UL);  // TODO: Support Content-Encoding in POST, PUT and PATCH, etc.
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
			if (this.finished)
				return true;

			this.finished = true;

			using (FileStream fs = File.OpenRead(this.precompressedFile.FullName))
			{
				long Pos = 0;
				long Length = fs.Length;
				byte[] Buffer = new byte[(int)Math.Min(Length, 65536)];

				while (Pos < Length)
				{
					NrBytes = (int)Math.Min(Length - Pos, 65536);
					NrBytes = await fs.ReadAsync(Buffer, 0, NrBytes);
					if (NrBytes <= 0)
						throw new IOException("Unexpected end of file.");

					if (!await this.uncompressedStream.EncodeAsync(ConstantBuffer, Buffer, 0, NrBytes, LastData))
						return false;

					Pos += NrBytes;
				}

				return true;
			}
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		/// <param name="EndOfData">If no more data is expected.</param>
		public override Task<bool> FlushAsync(bool EndOfData)
		{
			return this.uncompressedStream.FlushAsync(EndOfData);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override async Task<bool> ContentSentAsync()
		{
			await this.FlushAsync(true);
			return await this.uncompressedStream.ContentSentAsync();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.uncompressedStream?.Dispose();
			this.uncompressedStream = null;

			base.Dispose();
		}

		/// <summary>
		/// If encoding of data was invalid.
		/// </summary>
		public override bool InvalidEncoding => this.uncompressedStream.InvalidEncoding;

		/// <summary>
		/// If the transfer failed.
		/// </summary>
		public override bool TransferError => this.uncompressedStream.TransferError;
	}
}
