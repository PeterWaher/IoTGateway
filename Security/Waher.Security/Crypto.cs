using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Waher.Security
{
	/// <summary>
	/// Helper methods for encrypting and decrypting streams of data.
	/// </summary>
	public static class Crypto
	{
		/// <summary>
		/// Transforms a stream of data.
		/// </summary>
		/// <param name="Transform">Cryptographic transform.</param>
		/// <param name="Source">Source.</param>
		/// <param name="Destination">Destination</param>
		public static Task CryptoTransform(ICryptoTransform Transform, Stream Source, Stream Destination)
		{
			return CryptoTransform(Transform, Source, Destination, 65536);
		}

		/// <summary>
		/// Transforms a stream of data.
		/// </summary>
		/// <param name="Transform">Cryptographic transform.</param>
		/// <param name="Source">Source.</param>
		/// <param name="Destination">Destination</param>
		/// <param name="BufferSize">Intermediate buffer size. (Default=65536 bytes)</param>
		public static async Task CryptoTransform(ICryptoTransform Transform, Stream Source, Stream Destination, int BufferSize)
		{
			if (BufferSize <= 0)
				throw new ArgumentException("Invalid buffer size.", nameof(BufferSize));

			long l = Source.Length;

			BufferSize = (int)Math.Min(l, BufferSize);

			byte[] Input = new byte[BufferSize];
			byte[] Output = new byte[BufferSize];
			int j;

			while (l > 0)
			{
				j = (int)Math.Min(BufferSize, l);
				if (await Source.ReadAsync(Input, 0, j) != j)
					throw new IOException("Unexpected end of file.");

				l -= j;
				if (l <= 0)
				{
					Output = Transform.TransformFinalBlock(Input, 0, j);
					await Destination.WriteAsync(Output, 0, Output.Length);
				}
				else
				{
					j = Transform.TransformBlock(Input, 0, j, Output, 0);
					await Destination.WriteAsync(Output, 0, j);
				}
			}
		}

		/// <summary>
		/// Copies <paramref name="DataLen"/> number of bytes from <paramref name="From"/> to <paramref name="To"/>.
		/// </summary>
		/// <param name="From">Source data stream.</param>
		/// <param name="To">Destination data stream.</param>
		/// <param name="DataLen">Number of bytes to copy.</param>
		/// <returns>If copy was successful.</returns>
		public static async Task<bool> CopyAsync(Stream From, Stream To, long DataLen)
		{
			if (DataLen > 0)
			{
				int BufSize = (int)Math.Min(DataLen, 65536);
				byte[] Buffer = new byte[BufSize];

				while (DataLen > 0)
				{
					if (DataLen < BufSize)
						BufSize = (int)DataLen;

					if (await From.ReadAsync(Buffer, 0, BufSize) != BufSize)
						return false;

					await To.WriteAsync(Buffer, 0, BufSize);
					DataLen -= BufSize;
				}
			}

			return true;
		}

	}
}
