using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

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
		private readonly SemaphoreSlim semaphore;
		private long fileSize;
		private long filePosition;
		private bool disposed = false;

		private SingleFileQueue(string FileName, int MaxFileSize,
			ISerializerContext Context, SerialFile File, long FileSize)
		{
			this.fileName = FileName;
			this.maxFileSize = MaxFileSize;
			this.context = Context;
			this.file = File;
			this.fileSize = FileSize;
			this.filePosition = 0;
			this.semaphore = new SemaphoreSlim(1);
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
			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, Context, File, FileSize);
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
			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, Context, File, FileSize);
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
				this.semaphore.Dispose();
			}
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		public async Task Enqueue(object Item)
		{
			if (Item is null)
				throw new ArgumentNullException(nameof(Item));

			Type T = Item.GetType();
			IObjectSerializer Serializer = await this.context.GetObjectSerializer(T);
			BinarySerializer Writer = new BinarySerializer(string.Empty, Encoding.UTF8);

			await Serializer.Serialize(Writer, true, false, Item, null);

			byte[] TypeBinary = Encoding.UTF8.GetBytes(T.FullName);
			byte[] ItemBinary = Writer.GetSerialization();
			long TypeLen = TypeBinary.Length;
			long ItemLen = ItemBinary.Length;
			long c = (TypeLen + ItemLen + 16) & ~15;

			byte[] Payload = new byte[16 + c];

			Buffer.BlockCopy(BitConverter.GetBytes(TypeLen), 0, Payload, 0, 8);
			Buffer.BlockCopy(BitConverter.GetBytes(ItemLen), 0, Payload, 8, 8);

			Buffer.BlockCopy(TypeBinary, 0, Payload, 16, (int)TypeLen);                 // TODO: 64-bit
			Buffer.BlockCopy(ItemBinary, 0, Payload, 16 + (int)TypeLen, (int)ItemLen);  // TODO: 64-bit

			await this.semaphore.WaitAsync();
			try
			{
				await this.file.WriteBlock(Payload);
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Tries to dequeue an item from the queue. If the queue is empty, null is 
		/// returned. If all items have been dequeued, the file is cleared, to conserve
		/// disk space.
		/// </summary>
		/// <returns>Item if found, null otherwise.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public async Task<object> TryDequeue()
		{
			byte[] Payload;

			await this.semaphore.WaitAsync();
			try
			{
				if (this.filePosition >= this.fileSize)
					return null;

				KeyValuePair<byte[], long> Block = await this.file.ReadBlock(this.filePosition);
				Payload = Block.Key;
				this.filePosition = Block.Value;

				if (this.filePosition >= this.fileSize)
				{
					await this.file.Clear();

					this.filePosition = 0;
					this.fileSize = 0;
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			if (Payload.Length < 16)
				throw new InvalidOperationException("Block has been corrupted.");

			long TypeLen = BitConverter.ToInt64(Payload, 0);
			long ItemLen = BitConverter.ToInt64(Payload, 8);

			if (16 + TypeLen + ItemLen > Payload.Length)
				throw new InvalidOperationException("Block has been corrupted.");

			byte[] TypeBinary = new byte[TypeLen];
			byte[] ItemBinary = new byte[ItemLen];

			Buffer.BlockCopy(Payload, 16, TypeBinary, 0, (int)TypeLen);                 // TODO: 64-bit
			Buffer.BlockCopy(Payload, 16 + (int)TypeLen, ItemBinary, 0, (int)ItemLen);  // TODO: 64-bit

			string TypeName = Encoding.UTF8.GetString(TypeBinary);
			Type T = Types.GetType(TypeName)
				?? throw new InvalidOperationException("Type not found: " + TypeName);

			IObjectSerializer Serializer = await this.context.GetObjectSerializer(T);
			BinaryDeserializer Reader = new BinaryDeserializer(string.Empty, Encoding.UTF8, ItemBinary, 0);

			object Item = await Serializer.Deserialize(Reader, null, false);

			return Item;
		}
	}
}
