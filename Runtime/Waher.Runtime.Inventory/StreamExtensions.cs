using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Static class that extends the <see cref="Stream"/> class with secured buffer operations.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read, in which case an <see cref="IOException"/> is thrown.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="Buffer">Buffer to read into.</param>
		/// <param name="Pos">Position in buffer to read to.</param>
		/// <param name="Count">Number of bytes to read.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="Count"/> or <paramref name="Pos"/> are negative,
		/// or go beyond the range of the buffer.</exception>
		/// <exception cref="IOException">If not sufficient bytes exist to be read.</exception>
		public static void ReadAll(this Stream f, byte[] Buffer, int Pos, int Count)
		{
			if (TryReadAll(f, Buffer, Pos, Count) != Count)
				throw new IOException("Unexpected end of stream.");
		}

		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="Buffer">Buffer to read into.</param>
		/// <param name="Pos">Position in buffer to read to.</param>
		/// <param name="Count">Number of bytes to read.</param>
		/// <returns>The number of bytes reached.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="Count"/> or <paramref name="Pos"/> are negative,
		/// or go beyond the range of the buffer.</exception>
		public static int TryReadAll(this Stream f, byte[] Buffer, int Pos, int Count)
		{
			if (Count < 0)
				throw new ArgumentOutOfRangeException(nameof(Count));

			if (Pos < 0)
				throw new ArgumentOutOfRangeException(nameof(Count));

			if (Pos + Count > Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(Count));

			int c = 0;

			while (Count > 0)
			{
				int i = f.Read(Buffer, Pos, Count);
				if (i <= 0)
					return c;

				Pos += i;
				Count -= i;
				c += i;
			}

			return c;
		}

		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read, in which case an <see cref="IOException"/> is thrown.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="Buffer">Buffer to read into.</param>
		/// <param name="Pos">Position in buffer to read to.</param>
		/// <param name="Count">Number of bytes to read.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="Count"/> or <paramref name="Pos"/> are negative,
		/// or go beyond the range of the buffer.</exception>
		/// <exception cref="IOException">If not sufficient bytes exist to be read.</exception>
		public static async Task ReadAllAsync(this Stream f, byte[] Buffer, int Pos, int Count)
		{
			if (await TryReadAllAsync(f, Buffer, Pos, Count) != Count)
				throw new IOException("Unexpected end of stream.");
		}

		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="Buffer">Buffer to read into.</param>
		/// <param name="Pos">Position in buffer to read to.</param>
		/// <param name="Count">Number of bytes to read.</param>
		/// <returns>The number of bytes reached.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="Count"/> or <paramref name="Pos"/> are negative,
		/// or go beyond the range of the buffer.</exception>
		public static async Task<int> TryReadAllAsync(this Stream f, byte[] Buffer, int Pos, int Count)
		{
			if (Count < 0)
				throw new ArgumentOutOfRangeException(nameof(Count));

			if (Pos < 0)
				throw new ArgumentOutOfRangeException(nameof(Count));

			if (Pos + Count > Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(Count));

			int c = 0;

			while (Count > 0)
			{
				int i = await f.ReadAsync(Buffer, Pos, Count);
				if (i <= 0)
					return c;

				Pos += i;
				Count -= i;
				c += i;
			}

			return c;
		}
	}
}
