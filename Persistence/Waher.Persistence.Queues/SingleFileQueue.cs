using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// How to handle situations where the queue file has reached its maximum size. 
	/// </summary>
	public enum QueueThresholdMode
	{
		/// <summary>
		/// Ignore enqueued items when threshold has been reached.
		/// </summary>
		Ignore,

		/// <summary>
		/// Throw an exception when when threshold has been reached.
		/// </summary>
		Exception,

		/// <summary>
		/// Clears the file when threshold has been reached.
		/// </summary>
		Clear
	}

	/// <summary>
	/// Queue persisted into a single file.
	/// </summary>
	public partial class SingleFileQueue : IDisposable
	{
		private readonly LinkedList<TaskCompletionSource<object>> waiting = new LinkedList<TaskCompletionSource<object>>();
		private readonly SerializerCollection serializers;
		private readonly string fileName;
		private readonly int maxFileSize;
		private readonly SerialFile file;
		private readonly SemaphoreSlim semaphore;
		private readonly QueueThresholdMode thresholdMode;
		private long fileSize;
		private long filePosition;
		private bool disposed = false;

		private SingleFileQueue(string FileName, int MaxFileSize,
			QueueThresholdMode ThresholdMode, SerialFile File,
			long FileSize, SerializerCollection Serializers)
		{
			this.fileName = FileName;
			this.maxFileSize = MaxFileSize;
			this.thresholdMode = ThresholdMode;
			this.serializers = Serializers;
			this.file = File;
			this.fileSize = FileSize;
			this.filePosition = 0;
			this.semaphore = new SemaphoreSlim(1);
		}

		private SingleFileQueue(string FileName, int MaxFileSize,
			QueueThresholdMode ThresholdMode, SerialFile File,
#if COMPILED
			long FileSize, ISerializerContext SerializerContext, bool Compiled)
#else
			long FileSize, ISerializerContext SerializerContext)
#endif
		{
			this.fileName = FileName;
			this.maxFileSize = MaxFileSize;
			this.thresholdMode = ThresholdMode;
			this.file = File;
			this.fileSize = FileSize;
			this.filePosition = 0;
			this.semaphore = new SemaphoreSlim(1);

#if COMPILED
			this.serializers = new SerializerCollection(SerializerContext, Compiled);
#else
			this.serializers = new SerializerCollection(SerializerContext);
#endif
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		public static Task<SingleFileQueue> Create(string FileName,
			QueueThresholdMode ThresholdMode, SerializerCollection Serializers)
		{
			return Create(FileName, false, int.MaxValue, ThresholdMode, Serializers, (GetKeysMethod)null);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			QueueThresholdMode ThresholdMode, SerializerCollection Serializers,
			GetKeysMethod GetKeys)
		{
			return Create(FileName, Encrypted, int.MaxValue, ThresholdMode, Serializers, GetKeys);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, GetKeysMethod GetKeys)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, GetKeys);
			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Serializers);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Provider">Files database provider.</param>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			QueueThresholdMode ThresholdMode, FilesProvider Provider)
		{
			return Create(FileName, Encrypted, int.MaxValue, ThresholdMode, Provider);
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Provider">Files database provider.</param>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode, FilesProvider Provider)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, Provider);
			long FileSize = await File.GetLength();

#if COMPILED
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Provider, Provider.Compiled);
#else
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Provider);
#endif
		}

		/// <summary>
		/// Queue persisted into a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="Provider">Files database provider, used for encryption only.</param>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, FilesProvider Provider)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, Provider);
			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Serializers);
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
		/// Gets the current reading position in the file, and file size.
		/// </summary>
		/// <returns>File position and file size.</returns>
		public async Task<KeyValuePair<long, long>> GetFileState()
		{
			await this.semaphore.WaitAsync();
			try
			{
				return new KeyValuePair<long, long>(this.filePosition, this.fileSize);
			}
			finally
			{
				this.semaphore.Release();
			}
		}

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

				foreach (TaskCompletionSource<object> T in this.waiting)
					T.TrySetResult(null);

				this.waiting.Clear();
			}
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		public async Task<bool> Enqueue(object Item)
		{
			if (Item is null)
				throw new ArgumentNullException(nameof(Item));

			Type T = Item.GetType();
			IObjectSerializer Serializer = await this.serializers.GetObjectSerializer(T);
			BinarySerializer Writer = new BinarySerializer(string.Empty, Encoding.UTF8);

			await Serializer.Serialize(Writer, true, false, Item, null);

			byte[] TypeBinary = Encoding.UTF8.GetBytes(T.FullName);
			byte[] ItemBinary = Writer.GetSerialization();
			int TypeLen = TypeBinary.Length;
			int ItemLen = ItemBinary.Length;
			int c = TypeLen + ItemLen;

			byte[] Payload = new byte[8 + c];

			Buffer.BlockCopy(BitConverter.GetBytes(TypeLen), 0, Payload, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(ItemLen), 0, Payload, 4, 4);

			Buffer.BlockCopy(TypeBinary, 0, Payload, 8, TypeLen);
			Buffer.BlockCopy(ItemBinary, 0, Payload, 8 + TypeLen, ItemLen);

			await this.semaphore.WaitAsync();
			try
			{
				bool Forwarded;

				do
				{
					LinkedListNode<TaskCompletionSource<object>> First = this.waiting.First;

					if (First is null)
					{
						if (this.fileSize >= this.maxFileSize)
						{
							switch (this.thresholdMode)
							{
								case QueueThresholdMode.Ignore:
									return false;

								case QueueThresholdMode.Clear:
									await this.file.Clear();
									this.filePosition = 0;
									this.fileSize = 0;
									break;

								case QueueThresholdMode.Exception:
								default:
									throw new IOException("Queue file has reached its maximum size.");
							}
						}

						this.fileSize = await this.file.WriteBlock(Payload);
						Forwarded = true;
					}
					else
					{
						this.waiting.RemoveFirst();
						Forwarded = First.Value.TrySetResult(Item);
					}
				}
				while (!Forwarded);
			}
			finally
			{
				this.semaphore.Release();
			}

			return true;
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <returns>Dequeued item.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public Task<object> Dequeue()
		{
			return this.Dequeue(int.MaxValue);
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public async Task<object> Dequeue(int TimeoutMilliseconds)
		{
			if (TimeoutMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(TimeoutMilliseconds), "Value must be non-negative.");

			byte[] Payload;
			bool Released = false;

			await this.semaphore.WaitAsync();
			try
			{
				if (this.filePosition >= this.fileSize)
				{
					if (TimeoutMilliseconds == 0)
						return null;

					TaskCompletionSource<object> Waiter = new TaskCompletionSource<object>();
					LinkedListNode<TaskCompletionSource<object>> Node = this.waiting.AddLast(Waiter);

					this.semaphore.Release();
					Released = true;

					_ = Task.Delay(TimeoutMilliseconds).ContinueWith(async (_) =>
					{
						try
						{
							Waiter.TrySetResult(null);

							await this.semaphore.WaitAsync();
							try
							{
								if (!(Node.List is null))
									this.waiting.Remove(Node);
							}
							finally
							{
								this.semaphore.Release();
							}
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});

					return await Waiter.Task;
				}
				else
				{
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
			}
			finally
			{
				if (!Released)
					this.semaphore.Release();
			}

			if (Payload.Length < 8)
				throw new InvalidOperationException("Block has been corrupted.");

			int TypeLen = BitConverter.ToInt32(Payload, 0);
			int ItemLen = BitConverter.ToInt32(Payload, 4);

			if (8 + TypeLen + ItemLen > Payload.Length)
				throw new InvalidOperationException("Block has been corrupted.");

			byte[] TypeBinary = new byte[TypeLen];
			byte[] ItemBinary = new byte[ItemLen];

			Buffer.BlockCopy(Payload, 8, TypeBinary, 0, TypeLen);
			Buffer.BlockCopy(Payload, 8 + TypeLen, ItemBinary, 0, ItemLen);

			string TypeName = Encoding.UTF8.GetString(TypeBinary);
			Type T = Types.GetType(TypeName)
				?? throw new InvalidOperationException("Type not found: " + TypeName);

			IObjectSerializer Serializer = await this.serializers.GetObjectSerializer(T);
			BinaryDeserializer Reader = new BinaryDeserializer(string.Empty, Encoding.UTF8, ItemBinary, 0);

			object Item = await Serializer.Deserialize(Reader, null, false);

			return Item;
		}
	}
}
