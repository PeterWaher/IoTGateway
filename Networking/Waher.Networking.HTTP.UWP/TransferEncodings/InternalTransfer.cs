using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Waher.Networking.HTTP.TransferEncodings
{
	/// <summary>
	/// Transfer encoding for internal transfers of content
	/// </summary>
	public class InternalTransfer : TransferEncoding
	{
		private readonly MemoryStream ms;
		private readonly TaskCompletionSource<bool> task;
		private Timer timeoutTimer = null;

		/// <summary>
		/// Transfer encoding for internal transfers of content
		/// </summary>
		/// <param name="ms">Content will be output to this stream.</param>
		public InternalTransfer(MemoryStream ms)
			: base()
		{
			this.ms = ms;
			this.task = new TaskCompletionSource<bool>();
		}

		/// <summary>
		/// Is called when the content has all been sent to the encoder. The method sends any cached data to the client.
		/// </summary>
		public override void ContentSent()
		{
			this.task.TrySetResult(true);
			this.timeoutTimer?.Dispose();
			this.timeoutTimer = null;
		}

		/// <summary>
		/// Waits for all of the data to be returned.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Maximum time to wait, in milliseconds</param>
		public Task WaitUntilSent(int TimeoutMilliseconds)
		{
			this.timeoutTimer?.Dispose();
			this.timeoutTimer = null;

			this.timeoutTimer = new Timer((P) =>
			{
				this.task.TrySetException(new TimeoutException("Timed out while waiting for response."));
			}, null, TimeoutMilliseconds, Timeout.Infinite);

			return this.task.Task;
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
			NrAccepted = 0;
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
			this.ms.Write(Data, Offset, NrBytes);
		}

		/// <summary>
		/// Sends any remaining data to the client.
		/// </summary>
		public override void Flush()
		{
			this.ms.Flush();
		}
	}
}
