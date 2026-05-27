using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;

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
		Clear,

		/// <summary>
		/// Enqueuer waits for the queue to have resources to accept the item.
		/// </summary>
		Wait
	}

	/// <summary>
	/// Queue persisted into a single file.
	/// </summary>
	public class SingleFileQueue : IPersistedQueue
	{
		private readonly LinkedList<TaskCompletionSource<object>> waitingDequeuers = new LinkedList<TaskCompletionSource<object>>();
		private readonly LinkedList<TaskCompletionSource<bool>> waitingEnqueuers = new LinkedList<TaskCompletionSource<bool>>();
		private readonly SerializerCollection serializers;
		private readonly string fileName;
		private readonly int maxFileSize;
		private readonly SerialFile file;
		private readonly SemaphoreSlim semaphore;
		private readonly QueueThresholdMode thresholdMode;
		private readonly Profiler profiler;
		private readonly bool profiling;
		private ProfilerThread positionThread;
		private ProfilerThread sizeThread;
		private ProfilerThread trimThread;
		private ProfilerThread enqueueThread;
		private ProfilerThread dequeueThread;
		private long fileSize;
		private long filePosition;
		private int trimTimeout = 5000;
		private bool disposed = false;

		private SingleFileQueue(string FileName, int MaxFileSize,
			QueueThresholdMode ThresholdMode, SerialFile File,
			long FileSize, SerializerCollection Serializers,
			Profiler Profiler)
		{
			this.fileName = Path.GetFullPath(FileName);
			this.maxFileSize = MaxFileSize;
			this.thresholdMode = ThresholdMode;
			this.serializers = Serializers;
			this.file = File;
			this.fileSize = FileSize;
			this.filePosition = 0;
			this.profiler = Profiler;
			this.profiling = !(this.profiler is null);
			this.semaphore = new SemaphoreSlim(1);

			this.Start();
		}

		private SingleFileQueue(string FileName, int MaxFileSize,
			QueueThresholdMode ThresholdMode, SerialFile File, long FileSize,
#if COMPILED
			ISerializerContext SerializerContext, bool Compiled,
#else
			ISerializerContext SerializerContext,
#endif
			Profiler Profiler)
		{
			this.fileName = Path.GetFullPath(FileName);
			this.maxFileSize = MaxFileSize;
			this.thresholdMode = ThresholdMode;
			this.file = File;
			this.fileSize = FileSize;
			this.filePosition = 0;
			this.profiler = Profiler;
			this.profiling = !(this.profiler is null);
			this.semaphore = new SemaphoreSlim(1);

#if COMPILED
			this.serializers = new SerializerCollection(SerializerContext, Compiled);
#else
			this.serializers = new SerializerCollection(SerializerContext);
#endif
			this.Start();
		}

		private void Start()
		{
			if (this.profiling)
			{
				string Name = Path.GetFileName(this.fileName);

				this.positionThread = this.profiler.CreateThread("Pos(" + Name + ")", ProfilerThreadType.Analog);
				this.sizeThread = this.profiler.CreateThread("Size(" + Name + ")", ProfilerThreadType.Analog);
				this.trimThread = this.profiler.CreateThread("Trim(" + Name + ")", ProfilerThreadType.Binary);
				this.enqueueThread = this.profiler.CreateThread("Enqueue(" + Name + ")", ProfilerThreadType.Binary);
				this.dequeueThread = this.profiler.CreateThread("Dequeue(" + Name + ")", ProfilerThreadType.Binary);

				this.positionThread.Start();
				this.sizeThread.Start();
				this.trimThread.Start();
				this.enqueueThread.Start();
				this.dequeueThread.Start();

				this.positionThread.NewSample(this.filePosition);
				this.sizeThread.NewSample(this.fileSize);
			}
			else
			{
				this.positionThread = null;
				this.sizeThread = null;
				this.enqueueThread = null;
				this.dequeueThread = null;
				this.trimThread = null;
			}
		}

		private void Stop()
		{
			if (this.profiling)
			{
				this.positionThread.Stop();
				this.sizeThread.Stop();
				this.trimThread.Stop();
				this.enqueueThread.Stop();
				this.dequeueThread.Stop();
			}
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName,
			QueueThresholdMode ThresholdMode, SerializerCollection Serializers)
		{
			return Create(FileName, false, int.MaxValue, ThresholdMode, Serializers, (GetKeysMethod)null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			QueueThresholdMode ThresholdMode, SerializerCollection Serializers,
			GetKeysMethod GetKeys)
		{
			return Create(FileName, Encrypted, int.MaxValue, ThresholdMode, Serializers, GetKeys);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, GetKeysMethod GetKeys)
		{
			return Create(FileName, FileName, Encrypted, MaxFileSize, ThresholdMode, 
				Serializers, GetKeys, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, GetKeysMethod GetKeys, Profiler Profiler)
		{
			return Create(FileName, FileName, Encrypted, MaxFileSize, ThresholdMode,
				Serializers, GetKeys, Profiler);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="KeyFileName">File Name key to use when generating encryption keys.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<SingleFileQueue> Create(string FileName, string KeyFileName,
			bool Encrypted, int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, GetKeysMethod GetKeys, Profiler Profiler)
		{
			SerialFile File = await SerialFile.Create(FileName, KeyFileName, string.Empty, 
				Encrypted, GetKeys);

			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize,
				Serializers, Profiler);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			QueueThresholdMode ThresholdMode, FilesProvider Provider)
		{
			return Create(FileName, Encrypted, int.MaxValue, ThresholdMode, Provider);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode, FilesProvider Provider)
		{
			return Create(FileName, Encrypted, MaxFileSize, ThresholdMode, Provider, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode, FilesProvider Provider,
			Profiler Profiler)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, Provider);
			long FileSize = await File.GetLength();

#if COMPILED
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Provider, Provider.Compiled, Profiler);
#else
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Provider, Profiler);
#endif
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="Provider">Files database provider, used for encryption only.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, FilesProvider Provider)
		{
			return Create(FileName, Encrypted, MaxFileSize, ThresholdMode, Serializers,
				Provider, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="Provider">Files database provider, used for encryption only.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode,
			SerializerCollection Serializers, FilesProvider Provider,
			Profiler Profiler)
		{
			SerialFile File = await SerialFile.Create(FileName, string.Empty, Encrypted, Provider);
			long FileSize = await File.GetLength();

			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Serializers, Profiler);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			QueueThresholdMode ThresholdMode, ISerializerContext Context, FilesProvider Provider)
		{
			return Create(FileName, Encrypted, int.MaxValue, ThresholdMode, Context, Provider);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode, ISerializerContext Context,
			FilesProvider Provider)
		{
			return Create(FileName, FileName, Encrypted, MaxFileSize, ThresholdMode, Context,
				Provider, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<SingleFileQueue> Create(string FileName, bool Encrypted,
			int MaxFileSize, QueueThresholdMode ThresholdMode, ISerializerContext Context,
			FilesProvider Provider, Profiler Profiler)
		{
			return Create(FileName, FileName, Encrypted, MaxFileSize, ThresholdMode,
				Context, Provider, Profiler);
		}

		/// <summary>
		/// Creates a queue that perisists queued items in a single file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="KeyFileName">File Name key to use when generating encryption keys.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="ThresholdMode">How to handle enqueued items when the queue file has
		/// reached its maximum size.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<SingleFileQueue> Create(string FileName,string KeyFileName,
			bool Encrypted, int MaxFileSize, QueueThresholdMode ThresholdMode, 
			ISerializerContext Context, FilesProvider Provider, Profiler Profiler)
		{
			SerialFile File = await SerialFile.Create(FileName, KeyFileName, string.Empty, 
				Encrypted, Provider);

			long FileSize = await File.GetLength();

#if COMPILED
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Context, Provider.Compiled, Profiler);
#else
			return new SingleFileQueue(FileName, MaxFileSize, ThresholdMode, File, FileSize, Context, Profiler);
#endif
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
		/// Gets the number of dequeuers waiting for items to be queued.
		/// </summary>
		/// <returns>Number of dequeuers waiting for items.</returns>
		public async Task<int> GetNrDequeuers()
		{
			await this.semaphore.WaitAsync();
			try
			{
				return this.waitingDequeuers.Count;
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Gets the number of enqueuers waiting for space to be available to enqueue
		/// new items.
		/// </summary>
		/// <returns>Number of enqueuers waiting for space.</returns>
		public async Task<int> GetNrEnqueuers()
		{
			await this.semaphore.WaitAsync();
			try
			{
				return this.waitingEnqueuers.Count;
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Timeout, in milliseconds, before triggering a trimming of the file, when the 
		/// file has reached its maximum size. Default is 5000 ms.
		/// </summary>
		public int TrimTimeout
		{
			get => this.trimTimeout;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(this.TrimTimeout), "Value must be non-negative.");

				this.trimTimeout = value;
			}
		}

		/// <summary>
		/// Disposes the connection
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Disposes of the object, asynchronously.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				await this.semaphore.WaitAsync();

				if (this.filePosition > 0)
					await this.TrimLocked();

				this.file.Dispose();
				this.semaphore.Dispose();

				foreach (TaskCompletionSource<object> T in this.waitingDequeuers)
					T.TrySetResult(null);

				foreach (TaskCompletionSource<bool> T in this.waitingEnqueuers)
					T.TrySetResult(false);

				this.waitingDequeuers.Clear();
				this.waitingEnqueuers.Clear();

				this.Stop();
			}
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		public Task<bool> Enqueue(object Item)
		{
			return this.Enqueue(Item, int.MaxValue, true);
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If item was enqueued</returns>
		public Task<bool> Enqueue(object Item, int TimeoutMilliseconds)
		{
			return this.Enqueue(Item, TimeoutMilliseconds, true);
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="DoLock">If context should be locked (true), or if it has been
		/// locked already by the caller (false).</param>
		/// <returns>If item was enqueued</returns>
		private async Task<bool> Enqueue(object Item, int TimeoutMilliseconds, bool DoLock)
		{
			if (Item is null)
				throw new ArgumentNullException(nameof(Item));

			if (this.profiling)
				this.enqueueThread.High();

			Type T = Item.GetType();
			IObjectSerializer Serializer = await this.serializers.GetObjectSerializer(T);
			BinarySerializer Writer = new BinarySerializer(string.Empty, Encoding.UTF8);

			await Serializer.Serialize(Writer, true, false, Item, null);

			DateTime StartUtc = DateTime.UtcNow;
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

			bool Forwarded;
			bool TrimTimerStarted = false;
			TaskCompletionSource<bool> Wait = null;

			do
			{
				if (this.disposed)
				{
					if (this.profiling)
						this.enqueueThread.Low();

					return false;
				}

				if (!(Wait is null))
				{
					int Milliseconds = (int)(TimeoutMilliseconds - DateTime.UtcNow.Subtract(StartUtc).TotalMilliseconds);
					if (Milliseconds < 0)
					{
						if (this.profiling)
							this.enqueueThread.Low();

						return false;
					}

					_ = Task.Delay(Milliseconds).ContinueWith((_) =>
					{
						Wait?.TrySetResult(false);
						return Task.CompletedTask;
					});

					if (!await Wait.Task || this.disposed)
					{
						if (this.profiling)
							this.enqueueThread.Low();

						return false;
					}

					Wait = null;
				}

				if (DoLock)
					await this.semaphore.WaitAsync();
				try
				{
					LinkedListNode<TaskCompletionSource<object>> First = this.waitingDequeuers.First;

					if (First is null)
					{
						if (this.fileSize >= this.maxFileSize && DoLock)	// Avoid rule if call from Dequeue
						{
							if (this.filePosition > 0)
							{
								if (TrimTimerStarted)
								{
									Wait = new TaskCompletionSource<bool>();
									this.waitingEnqueuers.AddFirst(Wait);
								}
								else if (this.trimTimeout <= 0)
								{
									await this.TrimLocked();
									this.TriggerWaitingEnqueuersLocked();
								}
								else
								{
									TrimTimerStarted = true;
									Wait = new TaskCompletionSource<bool>();
									this.waitingEnqueuers.AddFirst(Wait);

									_ = Task.Delay(this.trimTimeout).ContinueWith(async (_) =>
									{
										TrimTimerStarted = false;
										await this.Trim();
										this.TriggerWaitingEnqueuersLocked();
									});
								}

								Forwarded = false;
								continue;
							}

							switch (this.thresholdMode)
							{
								case QueueThresholdMode.Ignore:
									if (this.profiling)
										this.enqueueThread.Low();

									return false;

								case QueueThresholdMode.Clear:
									await this.file.Clear();
									this.filePosition = 0;
									this.fileSize = 0;

									if (this.profiling)
									{
										this.positionThread.NewSample(0);
										this.sizeThread.NewSample(0);
									}

									this.TriggerWaitingEnqueuersLocked();
									break;

								case QueueThresholdMode.Wait:
									Wait = new TaskCompletionSource<bool>();
									this.waitingEnqueuers.AddFirst(Wait);
									Forwarded = false;
									continue;

								case QueueThresholdMode.Exception:
								default:
									throw new IOException("Queue file has reached its maximum size.");
							}
						}

						this.fileSize = await this.file.WriteBlock(Payload);

						if (this.profiling)
							this.sizeThread.NewSample(this.fileSize);

						Forwarded = true;
					}
					else
					{
						this.waitingDequeuers.RemoveFirst();
						Forwarded = First.Value.TrySetResult(Item);
					}
				}
				finally
				{
					if (DoLock)
						this.semaphore.Release();
				}
			}
			while (!Forwarded);

			if (this.profiling)
				this.enqueueThread.Low();

			return true;
		}

		private async Task Trim()
		{
			if (this.profiling)
				this.trimThread.High();

			await this.semaphore.WaitAsync();
			try
			{
				await this.TrimLocked();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				this.semaphore.Release();

				if (this.profiling)
					this.trimThread.Low();
			}
		}

		private async Task TrimLocked()
		{
			if (this.filePosition > 0)
			{
				long Pos = 0;

				while (this.filePosition < this.fileSize)
				{
					KeyValuePair<byte[], long> Block = await this.file.ReadBlock(this.filePosition);
					this.filePosition = Block.Value;

					if (this.profiling)
						this.positionThread.NewSample(this.filePosition);

					Pos = await this.file.WriteBlock(Block.Key, Pos);
				}

				await this.file.Truncate(Pos);
				this.filePosition = 0;
				this.fileSize = Pos;

				if (this.profiling)
				{
					this.positionThread.NewSample(0);
					this.sizeThread.NewSample(this.fileSize);
				}
			}
		}

		private void TriggerWaitingEnqueuersLocked()
		{
			if (this.waitingEnqueuers.First is null)
				return;

			ChunkedList<TaskCompletionSource<bool>> ToAwait = new ChunkedList<TaskCompletionSource<bool>>();
			ToAwait.AddRange(this.waitingEnqueuers);
			this.waitingEnqueuers.Clear();

			foreach (TaskCompletionSource<bool> T in ToAwait)
				T.TrySetResult(true);
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
			return this.Dequeue(int.MaxValue, true);
		}

		/// <summary>
		/// Tries to dequeue an item from the queue, if one exists. If an item is not 
		/// available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		public Task<object> TryDequeue()
		{
			return this.Dequeue(0, true);
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
		public Task<object> Dequeue(int TimeoutMilliseconds)
		{
			return this.Dequeue(TimeoutMilliseconds, true);
		}

		/// <summary>
		/// Returns the next item available to be dequeued, without dequeueing it.
		/// If an item is not available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		public Task<object> Peek()
		{
			return this.Dequeue(0, false);
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="RemoveItem">If the item should be removed from the queue, if found.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		internal async Task<object> Dequeue(int TimeoutMilliseconds, bool RemoveItem)
		{
			if (TimeoutMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(TimeoutMilliseconds), "Value must be non-negative.");

			if (this.disposed)
				return null;

			if (this.profiling)
				this.dequeueThread.High();

			byte[] Payload;
			bool Released = false;

			await this.semaphore.WaitAsync();
			try
			{
				if (this.filePosition >= this.fileSize)
				{
					if (TimeoutMilliseconds == 0)
					{
						if (this.profiling)
							this.dequeueThread.Low();

						return null;
					}

					TaskCompletionSource<object> Waiter = new TaskCompletionSource<object>();
					LinkedListNode<TaskCompletionSource<object>> Node = this.waitingDequeuers.AddLast(Waiter);

					this.semaphore.Release();
					Released = true;

					_ = Task.Delay(TimeoutMilliseconds).ContinueWith(async (_) =>
					{
						try
						{
							await this.semaphore.WaitAsync();
							try
							{
								if (!(Node.List is null))
									this.waitingDequeuers.Remove(Node);
							}
							finally
							{
								this.semaphore.Release();
							}

							Waiter.TrySetResult(null);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});

					object Result = await Waiter.Task;

					if (!RemoveItem)
						await this.Enqueue(Result, int.MaxValue, false);

					if (this.profiling)
						this.dequeueThread.Low();

					return Result;
				}
				else
				{
					KeyValuePair<byte[], long> Block = await this.file.ReadBlock(this.filePosition);
					Payload = Block.Key;

					if (RemoveItem)
					{
						this.filePosition = Block.Value;

						if (this.profiling)
							this.positionThread.NewSample(this.filePosition);

						if (this.filePosition >= this.fileSize)
						{
							await this.file.Clear();

							this.filePosition = 0;
							this.fileSize = 0;

							if (this.profiling)
							{
								this.positionThread.NewSample(0);
								this.sizeThread.NewSample(0);
							}
						}

						if (!(this.waitingEnqueuers.First is null))
							this.TriggerWaitingEnqueuersLocked();
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

			if (8 + TypeLen + ItemLen > Payload.Length || TypeLen < 0 || ItemLen < 0)
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

			if (this.profiling)
				this.dequeueThread.Low();

			return Item;
		}
	}
}
