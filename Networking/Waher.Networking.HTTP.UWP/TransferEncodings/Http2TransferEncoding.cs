﻿using System;
using System.Text;
using System.Threading;
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
		private readonly bool leaveStreamOpen;
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
		/// <param name="LeaveStreamOpen">If stream should be left open after transmission.</param>
		public Http2TransferEncoding(Http2Stream Stream, long? ContentLength,
			bool LeaveStreamOpen)
			: base(null, Stream.Connection)
		{
			this.stream = Stream;
			this.contentLength = ContentLength;

			this.bufferSize = Stream.Connection.LocalSettings.MaxFrameSize;
			if (ContentLength.HasValue && ContentLength.Value < this.bufferSize)
				this.bufferSize = (int)ContentLength.Value;

			this.buffer = new byte[this.bufferSize];
			this.leaveStreamOpen = LeaveStreamOpen;
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
		/// If stream should be left open after transmission.
		/// </summary>
		public bool LeaveStreamOpen => this.leaveStreamOpen;

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
			return Task.FromResult(0UL);
		}

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public override async Task<bool> EncodeAsync(byte[] Data, int Offset, int NrBytes)
		{
			if (this.ended)
				return true;

			if (NrBytes > 1000)
				this.dataEncoding = null;

			this.contentTransmitted += NrBytes;
			this.ended = this.contentLength.HasValue && this.contentTransmitted >= this.contentLength.Value;

			int NrToWrite;
			int NrWritten;
			bool Last;

			while (NrBytes > 0)
			{
				NrToWrite = Math.Min(NrBytes, this.bufferSize - this.pos);
				Last = this.ended && NrBytes == NrToWrite;

				if (NrToWrite == this.bufferSize ||     // Means this.pos == 0
					(Last && this.pos == 0))
				{
					NrWritten = await this.stream.TryWriteData(Data, Offset, NrToWrite, Last, this.dataEncoding);
					if (NrWritten < 0)
						return false;
				}
				else
				{
					Array.Copy(Data, Offset, this.buffer, this.pos, NrToWrite);
					this.pos += NrToWrite;

					if (this.pos == this.bufferSize || Last)
					{
						NrWritten = await this.stream.TryWriteData(this.buffer, 0, this.pos, Last, this.dataEncoding);
						if (NrWritten < 0)
							return false;

						if (NrWritten < this.pos)
						{
							int RestBytes = this.pos - NrWritten;

							Array.Copy(this.buffer, NrWritten, this.buffer, 0, RestBytes);
							this.pos = RestBytes;
						}
						else
							this.pos = 0;
					}
					
					NrWritten = NrToWrite;
				}

				Offset += NrWritten;
				NrBytes -= NrWritten;
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
				int i = 0;
				int j;

				while (i < this.pos)
				{
					j = await this.stream.TryWriteData(this.buffer, i, this.pos - i, this.ended, this.dataEncoding);
					if (j < 0)
						return false;

					i += j;
				}

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
				if (!this.leaveStreamOpen)
					this.ended = true;
			}

			if (this.pos > 0)
				return await this.FlushAsync();
			else
				return true;
		}
	}
}
