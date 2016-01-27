using System;
using System.IO;
using Waher.Events;

namespace Waher.Content
{
	/// <summary>
	/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
	/// </summary>
	public class TemporaryFile : FileStream
	{
		/// <summary>
		/// Default buffer size, in bytes.
		/// </summary>
		public const int DefaultBufferSize = 16384;

		private string fileName;

		/// <summary>
		/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
		/// </summary>
		public TemporaryFile()
			: this(Path.GetTempFileName(), DefaultBufferSize)
		{
		}

		/// <summary>
		/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
		/// </summary>
		/// <param name="FileName">Name of temporary file. Call <see cref="Path.GetTempFileName()"/> to get a new temporary file name.</param>
		public TemporaryFile(string FileName)
			: this(FileName, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
		/// </summary>
		/// <param name="FileName">Name of temporary file. Call <see cref="Path.GetTempFileName()"/> to get a new temporary file name.</param>
		/// <param name="BufferSize">Buffer size.</param>
		public TemporaryFile(string FileName, int BufferSize)
			: base(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, BufferSize, FileOptions.RandomAccess)
		{
			this.fileName = FileName;
		}

		/// <summary>
		/// Disposes of the object, and deletes the temporary file.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (this.fileName != null)
			{
				try
				{
					File.Delete(this.fileName);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				this.fileName = null;
			}
		}
	}
}