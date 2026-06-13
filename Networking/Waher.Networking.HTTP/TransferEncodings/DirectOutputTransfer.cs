using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HTTP2;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// A transfer encoding that outputs data as it is being generated, without any
	/// encoding, buffering or termination.
	/// </summary>
	public class DirectOutputTransfer : TransferEncoding
	{
		internal DirectOutputTransfer(IBinaryTransmission Output, HttpClientConnection ClientConnection,
			Encoding TextEncoding)
			: base(Output, ClientConnection)
		{
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
		/// Bits 0-31: Number of bytes accepted by the transfer encoding. If less than <paramref name="NrRead"/>, the rest is part of a separate message.
		/// Bit 32: If decoding has completed.
		/// Bit 33: If transmission to underlying stream failed.
		/// </returns>
		public override Task<ulong> DecodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
		{
			return Task.FromResult(0UL);
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
		public override Task<bool> EncodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrBytes, bool LastData)
		{
			return this.output.SendAsync(ConstantBuffer, Data, Offset, NrBytes);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> ContentSentAsync()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override Task<bool> FlushAsync(bool EndOfData)
		{
			return this.output.FlushAsync();
		}

		/// <summary>
		/// Is called when the header is complete, and before content is being transferred.
		/// </summary>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="ExpectContent">If content is expected.</param>
		public override async Task BeforeContentAsync(HttpResponse Response, bool ExpectContent)
		{
			if (Response.IsHttp1)
			{
				byte[] HeaderBin = Response.GenerateHttp11ResponseHeader(ExpectContent);

				await this.output.SendAsync(true, HeaderBin);

				this.clientConnection.Server.DataTransmitted(HeaderBin.Length);
			}
			else
			{
				byte[] HeaderBin = await Response.GenerateHttp2ResponseHeader(ExpectContent);
				if (!(HeaderBin is null))
					this.clientConnection.Server.DataTransmitted(HeaderBin.Length);
			}
		}
	}
}
