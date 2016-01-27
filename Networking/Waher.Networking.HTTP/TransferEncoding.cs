using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all transfer encodings.
	/// </summary>
	public abstract class TransferEncoding
	{
		/// <summary>
		/// Stream for decoded output.
		/// </summary>
		protected Stream output;

		/// <summary>
		/// If the received data was invalid.
		/// </summary>
		protected bool invalidEncoding = false;

		/// <summary>
		/// Base class for all transfer encodings.
		/// </summary>
		/// <param name="Output">Decoded output.</param>
		public TransferEncoding(Stream Output)
		{
			this.output = Output;
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
		public abstract bool Decode(byte[] Data, int Offset, int NrRead, out int NrAccepted);

		/// <summary>
		/// Is called when new binary data is to be sent and needs to be encoded.
		/// </summary>
		/// <param name="Data">Data buffer.</param>
		/// <param name="Offset">Offset where binary data begins.</param>
		/// <param name="NrBytes">Number of bytes to encode.</param>
		public abstract void Encode(byte[] Data, int Offset, int NrBytes);

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public abstract void Flush();

		/// <summary>
		/// If encoding of data was invalid.
		/// </summary>
		public bool InvalidEncoding
		{
			get { return this.invalidEncoding; }
		}

	}
}
