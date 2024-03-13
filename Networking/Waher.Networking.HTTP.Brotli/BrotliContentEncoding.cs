using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.Brotli
{
	/// <summary>
	/// Content-Encoding: br
	/// </summary>
	public class BrotliContentEncoding : IContentEncoding
	{
		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		public string Label => "br";

		/// <summary>
		/// How well the Content-Encoding handles the encoding specified by <paramref name="Label"/>.
		/// </summary>
		/// <param name="Label">Content-Encoding label.</param>
		/// <returns>How well the Content-Encoding is supported.</returns>
		public Grade Supports(string Label) => Label == this.Label ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets an encoder.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		/// <param name="ExpectedContentLength">Expected content length, if known.</param>
		/// <returns>Encoder</returns>
		public TransferEncoding GetEncoder(TransferEncoding Output, long? ExpectedContentLength)
		{
			BrotliEncoder Encoder = new BrotliEncoder(Output, ExpectedContentLength);
			Encoder.PrepareForCompression();
			return Encoder;
		}

		/// <summary>
		/// Class performing br encoding and decoding.
		/// </summary>
		private class BrotliEncoder : TransferEncoding
		{
			private TransferEncoding? uncompressedStream;
			private MemoryStream? ms = null;
			private BrotliStream? brEncoder = null;
			private BrotliStream? brDecoder = null;
			private long? bytesLeft;
			private int pos = 0;
			private bool dataWritten = false;
			private bool finished = false;

			/// <summary>
			/// Class performing br encoding and decoding.
			/// </summary>
			/// <param name="UncompressedStream">Output stream.</param>
			/// <param name="ExpectedContentLength">Expected content length, if known.</param>
			public BrotliEncoder(TransferEncoding UncompressedStream, long? ExpectedContentLength)
				: base(null, UncompressedStream)
			{
				this.uncompressedStream = UncompressedStream;
				this.bytesLeft = ExpectedContentLength;
			}

			/// <summary>
			/// Prepares the encoder for compression
			/// </summary>
			public void PrepareForCompression()
			{
				this.ms = new MemoryStream();
				this.brEncoder = new BrotliStream(this.ms, CompressionMode.Compress, true);
			}

			/// <summary>
			/// Is called when the header is complete, and before content is being transferred.
			/// </summary>
			/// <param name="Response">HTTP Response object.</param>
			/// <param name="ExpectContent">If content is expected.</param>
			public override Task BeforeContentAsync(HttpResponse Response, bool ExpectContent)
			{
				return this.uncompressedStream!.BeforeContentAsync(Response, ExpectContent);
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
				return Task.FromResult(0x100000000UL);  // TODO: Support Content-Encoding in POST, PUT and PATCH, etc.
			}

			/// <summary>
			/// Is called when new binary data is to be sent and needs to be encoded.
			/// </summary>
			/// <param name="Data">Data buffer.</param>
			/// <param name="Offset">Offset where binary data begins.</param>
			/// <param name="NrBytes">Number of bytes to encode.</param>
			public override async Task<bool> EncodeAsync(byte[] Data, int Offset, int NrBytes)
			{
				if (NrBytes > 0)
				{
					if (this.finished)
						return false;

					this.dataWritten = true;

					if (this.bytesLeft.HasValue)
					{
						if (NrBytes > this.bytesLeft.Value)
							NrBytes = (int)this.bytesLeft.Value;

						await this.brEncoder!.WriteAsync(Data, Offset, NrBytes);

						this.bytesLeft -= NrBytes;
						if (this.bytesLeft <= 0)
						{
							this.finished = true;
							await this.FlushAsync();
						}
					}
					else
						await this.brEncoder!.WriteAsync(Data, Offset, NrBytes);
				}

				return true;
			}

			/// <summary>
			/// Sends any remaining data to the client.
			/// </summary>
			public override async Task<bool> FlushAsync()
			{
				if (this.dataWritten)
				{
					await this.brEncoder!.FlushAsync();
					this.dataWritten = false;

					byte[] Data = this.ms!.ToArray();
					int c = Data.Length;
					if (this.pos < c)
					{
						c -= this.pos;
						if (!await this.uncompressedStream!.EncodeAsync(Data, this.pos, c))
							return false;
						this.pos += c;
					}

					return await this.uncompressedStream!.FlushAsync();
				}

				return true;
			}

			/// <summary>
			/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
			/// </summary>
			public override async Task<bool> ContentSentAsync()
			{
				await this.FlushAsync();
				return await this.uncompressedStream!.ContentSentAsync();
			}

			/// <summary>
			/// <see cref="IDisposable.Dispose"/>
			/// </summary>
			public override void Dispose()
			{
				this.uncompressedStream?.Dispose();
				this.uncompressedStream = null;

				this.brEncoder?.Dispose();
				this.brEncoder = null;

				this.brDecoder?.Dispose();
				this.brDecoder = null;

				this.ms?.Dispose();
				this.ms = null;

				base.Dispose();
			}

			/// <summary>
			/// If encoding of data was invalid.
			/// </summary>
			public override bool InvalidEncoding => this.uncompressedStream!.InvalidEncoding;

			/// <summary>
			/// If the transfer failed.
			/// </summary>
			public override bool TransferError => this.uncompressedStream!.TransferError;
		}
	}
}
