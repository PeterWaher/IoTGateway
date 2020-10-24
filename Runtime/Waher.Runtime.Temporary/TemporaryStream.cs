using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Temporary
{
	/// <summary>
	/// Manages a temporary stream. Contents is kept in-memory, if below a memory threshold, and switched to a temporary file
	/// if crossing threshold.
	/// </summary>
	public class TemporaryStream : Stream
	{
		private static int defaultThreasholdBytes = 1024 * 1024;

		/// <summary>
		/// Default threashold before switching to temporary files from in-memory streams.
		/// </summary>
		public static int DefaultThreasholdBytes
		{
			get => defaultThreasholdBytes;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Must be positive.", nameof(DefaultThreasholdBytes));

				defaultThreasholdBytes = value;
			}
		}

		private readonly int thresholdBytes;
		private Stream stream;
		private bool checkSize;

		/// <summary>
		/// Manages a temporary stream. Contents is kept in-memory, if below a memory threshold, and switched to a temporary file
		/// if crossing threshold.
		/// </summary>
		public TemporaryStream()
			: this(DefaultThreasholdBytes)
		{
		}

		/// <summary>
		/// Manages a temporary stream. Contents is kept in-memory, if below a memory threshold, and switched to a temporary file
		/// if crossing threshold.
		/// </summary>
		/// <param name="ThresholdBytes">Threshold, in bytes (default=1048576 bytes, set in <see cref="DefaultThreasholdBytes"/>).</param>
		public TemporaryStream(int ThresholdBytes)
		{
			this.thresholdBytes = ThresholdBytes;
			this.stream = new MemoryStream();
			this.checkSize = true;
		}

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		public override long Position
		{
			get => this.stream.Position;
			set => this.stream.Position = value;
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		public override long Length => this.stream.Length;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current
		/// stream supports writing.
		/// </summary>
		public override bool CanWrite => this.stream.CanWrite;

		/// <summary>
		/// Gets a value that determines whether the current stream can time out.
		/// </summary>
		public override bool CanTimeout => this.stream.CanTimeout;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current
		/// stream supports seeking.
		/// </summary>
		public override bool CanSeek => this.stream.CanSeek;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current
		/// stream supports reading.
		/// </summary>
		public override bool CanRead => this.stream.CanRead;

		/// <summary>
		/// Gets or sets a value, in miliseconds, that determines how long the stream will
		/// attempt to read before timing out.
		/// </summary>
		public override int ReadTimeout
		{
			get => this.stream.ReadTimeout;
			set => this.stream.ReadTimeout = value;
		}

		/// <summary>
		/// Gets or sets a value, in miliseconds, that determines how long the stream will
		/// attempt to write before timing out.
		/// </summary>
		public override int WriteTimeout
		{
			get => this.stream.WriteTimeout;
			set => this.stream.WriteTimeout = value;
		}

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another
		/// stream, using a specified buffer size and cancellation token.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The
		/// default size is 81920.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. 
		/// The default value is <see cref="CancellationToken.None"/>.</param>
		public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			if (this.checkSize && this.stream.Position + bufferSize > this.thresholdBytes)
				await this.SwitchToFileAsync(cancellationToken);

			await this.stream.CopyToAsync(destination, bufferSize, cancellationToken);
		}

		private async Task SwitchToFileAsync(CancellationToken cancellationToken)
		{
			TemporaryFile File = new TemporaryFile();
			try
			{
				long Pos = this.stream.Position;
				this.stream.Position = 0;
				await this.stream.CopyToAsync(File, 81920, cancellationToken);

				File.Position = Pos;
				this.stream.Dispose();
				this.stream = File;

				this.checkSize = false;
			}
			catch (Exception ex)
			{
				ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		private void SwitchToFile()
		{
			TemporaryFile File = new TemporaryFile();
			try
			{
				long Pos = this.stream.Position;
				this.stream.Position = 0;
				this.stream.CopyTo(File);

				File.Position = Pos;
				this.stream.Dispose();
				this.stream = File;

				this.checkSize = false;
			}
			catch (Exception ex)
			{
				ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		/// <summary>
		/// When overridden in a derived class, clears all buffers for this stream and causes
		/// any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush()
		{
			this.stream.Flush();
		}

		/// <summary>
		/// Asynchronously clears all buffers for this stream, causes any buffered data to
		/// be written to the underlying device, and monitors cancellation requests.
		/// </summary>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return this.stream.FlushAsync(cancellationToken);
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current
		/// stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified
		/// byte array with the values between offset and (offset + count - 1) replaced by
		/// the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read
		/// from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number
		/// of bytes requested if that many bytes are not currently available, or zero (0)
		/// if the end of the stream has been reached.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.stream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream, advances the
		/// position within the stream by the number of bytes read, and monitors cancellation
		/// requests.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A task that represents the asynchronous read operation. The value of the TResult
		/// parameter contains the total number of bytes read into the buffer. The result
		/// value can be less than the number of bytes requested if the number of bytes currently
		/// available is less than the requested number, or it can be 0 (zero) if the end
		/// of the stream has been reached.</returns>
		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one
		/// byte, or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
		public override int ReadByte()
		{
			return this.stream.ReadByte();
		}

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain
		/// the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.stream.Seek(offset, origin);
		}

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		public override void SetLength(long value)
		{
			if (this.checkSize && value > this.thresholdBytes)
				this.SwitchToFile();

			this.stream.SetLength(value);
		}

		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current
		/// stream and advances the current position within this stream by the number of
		/// bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current
		/// stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current
		/// stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.checkSize && this.stream.Position + count > this.thresholdBytes)
				this.SwitchToFile();

			this.stream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream, advances the
		/// current position within this stream by the number of bytes written, and monitors
		/// cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the
		/// stream.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (this.checkSize && this.stream.Position + count > this.thresholdBytes)
				await this.SwitchToFileAsync(cancellationToken);

			await this.stream.WriteAsync(buffer, offset, count, cancellationToken);
		}

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position
		/// within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		public override void WriteByte(byte value)
		{
			if (this.checkSize && this.stream.Position + 1 > this.thresholdBytes)
				this.SwitchToFile();

			this.stream.WriteByte(value);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the System.IO.Stream and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged
		/// resources.</param>
		protected override void Dispose(bool disposing)
		{
			this.stream?.Dispose();
			this.stream = null;
		}
	}
}
