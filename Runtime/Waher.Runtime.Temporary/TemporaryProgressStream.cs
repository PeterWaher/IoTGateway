using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Temporary
{
	/// <summary>
	/// Class managing the contents of a temporary stream. When the class is disposed, any temporary file is deleted.
	/// The class provides an event letting the caller receive events on progress.
	/// </summary>
	public class TemporaryProgressStream : TemporaryStream
	{
		private long nrReadTotal = 0;
		private long nrWrittenTotal = 0;

		/// <summary>
		/// Class managing the contents of a temporary stream. When the class is disposed, any temporary file is deleted.
		/// The class provides an event letting the caller receive events on progress.
		/// </summary>
		public TemporaryProgressStream()
			: base()
		{
		}

		/// <summary>
		/// Class managing the contents of a temporary stream. When the class is disposed, any temporary file is deleted.
		/// The class provides an event letting the caller receive events on progress.
		/// </summary>
		/// <param name="BufferSize">Buffer size.</param>
		public TemporaryProgressStream(int BufferSize)
			: base(BufferSize)
		{
		}

		/// <summary>
		/// Event raised when data has been read from the file.
		/// </summary>
		public event EventHandlerAsync<IoBytesEventArgs> OnRead;

		/// <summary>
		/// Event raised when data has been written to the file.
		/// </summary>
		public event EventHandlerAsync<IoBytesEventArgs> OnWrite;

		/// <inheritdoc/>
		public override void Write(byte[] array, int offset, int count)
		{
			base.Write(array, offset, count);

			this.nrWrittenTotal += count;
			this.OnWrite.Raise(this, new IoBytesEventArgs(count, this.nrWrittenTotal));
		}

		/// <inheritdoc/>
		public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			await base.WriteAsync(buffer, offset, count, cancellationToken);

			this.nrWrittenTotal += count;
			await this.OnWrite.Raise(this, new IoBytesEventArgs(count, this.nrWrittenTotal));
		}

		/// <inheritdoc/>
		public override void WriteByte(byte value)
		{
			base.WriteByte(value);

			this.nrWrittenTotal++;
			this.OnWrite.Raise(this, new IoBytesEventArgs(1, this.nrWrittenTotal));
		}

		/// <inheritdoc/>
		public override int Read(byte[] array, int offset, int count)
		{
			int i = base.Read(array, offset, count);

			if (i > 0)
			{
				this.nrReadTotal += i;
				this.OnRead.Raise(this, new IoBytesEventArgs(i, this.nrReadTotal));
			}

			return i;
		}

		/// <inheritdoc/>
		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			int i = await base.ReadAsync(buffer, offset, count, cancellationToken);

			if (i > 0)
			{
				this.nrReadTotal += i;
				await this.OnRead.Raise(this, new IoBytesEventArgs(i, this.nrReadTotal));
			}

			return i;
		}

		/// <inheritdoc/>
		public override int ReadByte()
		{
			int i = base.ReadByte();
			if (i < 0)
				return i;

			this.nrReadTotal++;
			this.OnRead.Raise(this, new IoBytesEventArgs(i, this.nrReadTotal));

			return i;
		}

	}
}