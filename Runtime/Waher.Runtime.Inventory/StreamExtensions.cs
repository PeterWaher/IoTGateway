using System;
using System.Drawing;
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
		/// Reads all available data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read, in which case an <see cref="IOException"/> is thrown.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <exception cref="IOException">If not sufficient bytes exist to be read.</exception>
		/// <exception cref="InternalBufferOverflowException">If more data is available than can be read
		/// into a byte buffer.</exception>
		public static async Task<byte[]> ReadAllAsync(this Stream f)
		{
			long Len = f.Length;
			long Left = Len - f.Position;
			if (Left > int.MaxValue)
			{
				if (f is FileStream fs)
					throw new InternalBufferOverflowException("File too large: " + fs.Name);
				else
					throw new InternalBufferOverflowException("Stream too large.");
			}

			int Count = (int)Left;
			byte[] Buffer = new byte[Count];

			await f.ReadAllAsync(Buffer, 0, Count);

			return Buffer;
		}

		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read, in which case an <see cref="IOException"/> is thrown.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="Buffer">Buffer to read into.</param>
		/// <exception cref="IOException">If not sufficient bytes exist to be read.</exception>
		public static Task ReadAllAsync(this Stream f, byte[] Buffer)
		{
			return f.ReadAllAsync(Buffer, 0, Buffer.Length);
		}

		/// <summary>
		/// Reads data from a stream to a buffer. Multiple read operations are performed until
		/// either all requested bytes have been read, or no more bytes are available to be
		/// read, in which case an <see cref="IOException"/> is thrown.
		/// </summary>
		/// <param name="f">Stream to read from.</param>
		/// <param name="NrBytes">Number of bytes to read.</param>
		/// <exception cref="IOException">If not sufficient bytes exist to be read.</exception>
		public static async Task<byte[]> ReadAllAsync(this Stream f, int NrBytes)
		{
			byte[] Buffer = new byte[NrBytes];
			await f.ReadAllAsync(Buffer, 0, NrBytes);
			return Buffer;
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
		/// <returns>The number of bytes reached.</returns>
		public static Task<int> TryReadAllAsync(this Stream f, byte[] Buffer)
		{
			return f.TryReadAllAsync(Buffer, 0, Buffer.Length);
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
