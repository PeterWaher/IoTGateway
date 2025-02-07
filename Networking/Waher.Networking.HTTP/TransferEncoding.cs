using System;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all transfer encodings.
	/// </summary>
	public abstract class TransferEncoding : IDisposable
	{
		/// <summary>
		/// Stream for decoded output.
		/// </summary>
		protected IBinaryTransmission output;

		/// <summary>
		/// If the received data was invalid.
		/// </summary>
		protected bool invalidEncoding = false;

		/// <summary>
		/// If the transfer failed.
		/// </summary>
		protected bool transferError = false;

		/// <summary>
		/// Client connection.
		/// </summary>
		internal HttpClientConnection clientConnection;

		/// <summary>
		/// Base class for all transfer encodings.
		/// </summary>
		public TransferEncoding()
		{
		}

		/// <summary>
		/// Base class for all transfer encodings.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ClientStream">Underlying client stream</param>
		public TransferEncoding(IBinaryTransmission Output, TransferEncoding ClientStream)
			: this(Output, ClientStream.clientConnection)
		{
		}

		/// <summary>
		/// Base class for all transfer encodings.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		/// <param name="ClientConnection">Client connection.</param>
		internal TransferEncoding(IBinaryTransmission Output, HttpClientConnection ClientConnection)
		{
			this.output = Output;
			this.clientConnection = ClientConnection;
		}

		/// <summary>
		/// Is called when the header is complete, and before content is being transferred.
		/// </summary>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="ExpectContent">If content is expected.</param>
		public virtual Task BeforeContentAsync(HttpResponse Response, bool ExpectContent)
		{
			return Task.CompletedTask;
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
		public abstract Task<ulong> DecodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrRead);

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		/// <param name="LastData">If no more data is expected.</param>
		public abstract Task<bool> EncodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrBytes, bool LastData);

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		/// <param name="EndOfData">If no more data is expected.</param>
		public abstract Task<bool> FlushAsync(bool EndOfData);

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public abstract Task<bool> ContentSentAsync();

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// If encoding of data was invalid.
		/// </summary>
		public virtual bool InvalidEncoding => this.invalidEncoding;

		/// <summary>
		/// If the transfer failed.
		/// </summary>
		public virtual bool TransferError => this.transferError;

	}
}
