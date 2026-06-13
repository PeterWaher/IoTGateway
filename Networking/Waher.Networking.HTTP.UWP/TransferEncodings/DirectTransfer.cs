using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// A transfer encoding that outputs data as it is being generated, without any
	/// encoding, buffering or termination.
	/// </summary>
	public class DirectTransfer : TransferEncoding
	{
		internal DirectTransfer(IBinaryTransmission Output, HttpClientConnection ClientConnection,
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
			throw new System.NotImplementedException();
		}

		public override Task<bool> ContentSentAsync()
		{
			throw new System.NotImplementedException();
		}

		public override Task<bool> EncodeAsync(bool ConstantBuffer, byte[] Data, int Offset, int NrBytes, bool LastData)
		{
			throw new System.NotImplementedException();
		}

		public override Task<bool> FlushAsync(bool EndOfData)
		{
			throw new System.NotImplementedException();
		}
	}
}
