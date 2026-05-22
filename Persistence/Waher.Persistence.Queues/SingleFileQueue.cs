using System;
using System.Threading.Tasks;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Queue persisted into a single file.
	/// </summary>
	public class SingleFileQueue : IDisposable
	{
		private readonly ISerializerContext context;
		private readonly string fileName;
		private readonly int maxFileSize;
		private readonly SerialFile file;
		private bool disposed = false;

		private SingleFileQueue(string FileName, int MaxFileSize, 
			ISerializerContext Context, SerialFile File)
		{
			this.fileName = FileName;
			this.maxFileSize = MaxFileSize;
			this.context = Context;
			this.file = File;
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Context">Serialization context.</param>
		public static Task<SingleFileQueue> Create(string FileName, ISerializerContext Context)
		{
			return Create(FileName, false, int.MaxValue, Context, (GetKeysMethod)null);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted, ISerializerContext Context, 
			GetKeysMethod GetKeys)
		{
			return Create(FileName, Encrypted, int.MaxValue, Context, GetKeys);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted, int MaxFileSize, 
			ISerializerContext Context, GetKeysMethod GetKeys)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, GetKeys);
			return new SingleFileQueue(FileName, MaxFileSize, Context, File);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted, ISerializerContext Context,
			FilesProvider Provider)
		{
			return Create(FileName, Encrypted, int.MaxValue, Context, Provider);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted, int MaxFileSize,
			ISerializerContext Context, FilesProvider Provider)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, Provider);
			return new SingleFileQueue(FileName, MaxFileSize, Context, File);
		}

		/// <summary>
		/// File name.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Maximum file size, in bytes.
		/// </summary>
		public int MaxFileSize => this.maxFileSize;

		/// <summary>
		/// Disposes the connection
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.file.Dispose();
			}
		}
	}
}
