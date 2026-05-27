using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Profiling;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Queue persisted into one or more files.
	/// </summary>
	public class MultiFileQueue : IPersistedQueue
	{
		private readonly LinkedList<QueuedFile> files = new LinkedList<QueuedFile>();
		private readonly SerializerCollection serializers;
		private readonly ISerializerContext context;
		private readonly GetKeysMethod getKeys;
		private readonly FilesProvider provider;
		private readonly SemaphoreSlim semaphore;
		private readonly Profiler profiler;
		private readonly string folderName;
		private readonly int maxFileSize;
		private readonly bool encrypted = false;
		private int fileCount;
		private bool disposed = false;

		private MultiFileQueue(string FolderName, int MaxFileSize, bool Encrypted,
			SerializerCollection Serializers, GetKeysMethod GetKeys, Profiler Profiler)
		{
			this.folderName = Path.GetFullPath(FolderName);
			this.maxFileSize = MaxFileSize;
			this.encrypted = Encrypted;
			this.serializers = Serializers;
			this.getKeys = GetKeys;
			this.context = null;
			this.provider = null;
			this.profiler = Profiler;
			this.semaphore = new SemaphoreSlim(1);
		}

		private MultiFileQueue(string FolderName, int MaxFileSize, bool Encrypted,
			ISerializerContext Context, FilesProvider Provider, Profiler Profiler)
		{
			this.folderName = Path.GetFullPath(FolderName);
			this.maxFileSize = MaxFileSize;
			this.encrypted = Encrypted;
			this.serializers = null;
			this.getKeys = null;
			this.context = Context;
			this.provider = Provider;
			this.profiler = Profiler;
			this.semaphore = new SemaphoreSlim(1);
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, SerializerCollection Serializers, GetKeysMethod GetKeys)
		{
			return Create(FolderName, Encrypted, MaxFileSize, Serializers, GetKeys, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Serializers">Collection of serializers.</param>
		/// <param name="GetKeys">Method that provides encryption keys.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, SerializerCollection Serializers, GetKeysMethod GetKeys,
			Profiler Profiler)
		{
			MultiFileQueue Result = new MultiFileQueue(FolderName, MaxFileSize, Encrypted,
				Serializers, GetKeys, Profiler);

			await Result.PrepareQueue();

			return Result;
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, FilesProvider Provider)
		{
			return Create(FolderName, Encrypted, MaxFileSize, Provider, (Profiler)null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, FilesProvider Provider, Profiler Profiler)
		{
			return Create(FolderName, Encrypted, MaxFileSize, Provider, Provider, Profiler);
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <returns>Queue object instance.</returns>
		public static Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, ISerializerContext Context, FilesProvider Provider)
		{
			return Create(FolderName, Encrypted, MaxFileSize, Context, Provider, null);
		}

		/// <summary>
		/// Creates a queue that perisists queued items using multiple files in a folder.
		/// </summary>
		/// <param name="FolderName">Folder name</param>
		/// <param name="Encrypted">If files should be encrypted.</param>
		/// <param name="MaxFileSize">Maximum file size, in bytes.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Provider">Files database provider.</param>
		/// <param name="Profiler">Optional profiler.</param>
		/// <returns>Queue object instance.</returns>
		public static async Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, ISerializerContext Context, FilesProvider Provider,
			Profiler Profiler)
		{
			MultiFileQueue Result = new MultiFileQueue(FolderName, MaxFileSize, Encrypted,
				Context, Provider, Profiler);

			try
			{
				await Result.PrepareQueue();

				return Result;
			}
			catch (Exception ex)
			{
				await Result.DisposeAsync();
				ExceptionDispatchInfo.Capture(ex).Throw();
				throw ex;   // Only to avoid compiler error. Will never be reached.
			}
		}

		private async Task PrepareQueue()
		{
			string[] FileNames = GetFilesOrderedByCreationTime(this.folderName);
			int i;

			this.fileCount = FileNames.Length;

			if (this.fileCount == 0)
			{
				FileNames = new string[] { Path.Combine(this.folderName, Guid.NewGuid().ToString() + ".queue") };
				this.fileCount = 1;
			}

			if (this.fileCount == 1)
			{
				this.files.AddLast(new QueuedFile()
				{
					FileName = FileNames[0],
					Queue = await this.CreateSingleFileQueue(FileNames[0])
				});
			}
			else
			{
				for (i = 0; i < this.fileCount; i++)
				{
					if (i == 0 || i == this.fileCount - 1)
					{
						this.files.AddLast(new QueuedFile()
						{
							FileName = FileNames[i],
							Queue = await this.CreateSingleFileQueue(FileNames[i])
						});
					}
					else
					{
						this.files.AddLast(new QueuedFile()
						{
							FileName = FileNames[i]
						});
					}
				}
			}
		}

		private class QueuedFile
		{
			public string FileName;
			public SingleFileQueue Queue;
		}

		private async Task<SingleFileQueue> CreateSingleFileQueue(string FileName)
		{
			if (this.provider is null)
			{
				return await SingleFileQueue.Create(FileName, this.folderName, this.encrypted, 
					this.maxFileSize, QueueThresholdMode.Ignore, this.serializers, this.getKeys, 
					this.profiler);
			}
			else
			{
				return await SingleFileQueue.Create(FileName, this.folderName, this.encrypted, 
					this.maxFileSize, QueueThresholdMode.Ignore, this.context, this.provider, 
					this.profiler);
			}
		}

		private static string[] GetFilesOrderedByCreationTime(string FolderName)
		{
			if (!Directory.Exists(FolderName))
				Directory.CreateDirectory(FolderName);

			string[] FileNames = Directory.GetFiles(FolderName, "*.queue", SearchOption.TopDirectoryOnly);
			int i, c = FileNames.Length;
			KeyValuePair<string, DateTime>[] FilesPerCreationTime = new KeyValuePair<string, DateTime>[c];

			for (i = 0; i < c; i++)
				FilesPerCreationTime[i] = new KeyValuePair<string, DateTime>(FileNames[i], File.GetCreationTimeUtc(FileNames[i]));

			Array.Sort(FilesPerCreationTime, (x, y) => x.Value.CompareTo(y.Value));

			for (i = 0; i < c; i++)
				FileNames[i] = FilesPerCreationTime[i].Key;

			return FileNames;
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

				foreach (QueuedFile File in this.files)
				{
					if (!(File.Queue is null))
					{
						try
						{
							await File.Queue.DisposeAsync();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}

				this.files.Clear();

				this.semaphore.Dispose();
			}
		}

		/// <summary>
		/// Gets the number of files used by the queue.
		/// </summary>
		/// <returns>Number of files.</returns>
		public async Task<int> GetFileCount()
		{
			await this.semaphore.WaitAsync();
			try
			{
				return this.fileCount;
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
				QueuedFile File = this.files.First.Value;

				if (File.Queue is null)
					File.Queue = await this.CreateSingleFileQueue(File.FileName);

				return await File.Queue.GetNrDequeuers();
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
				QueuedFile File = this.files.Last.Value;

				if (File.Queue is null)
					File.Queue = await this.CreateSingleFileQueue(File.FileName);

				return await File.Queue.GetNrEnqueuers();
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Gets the current reading position in the file, and file size.
		/// </summary>
		/// <returns>File position and file size of first file, followed by
		/// File position and file size of last file.</returns>
		public async Task<Tuple<long, long, long, long>> GetFileState()
		{
			await this.semaphore.WaitAsync();
			try
			{
				QueuedFile File = this.files.First.Value;

				if (File.Queue is null)
					File.Queue = await this.CreateSingleFileQueue(File.FileName);

				KeyValuePair<long, long> FirstFileState = await File.Queue.GetFileState();

				File = this.files.Last.Value;

				if (File.Queue is null)
					File.Queue = await this.CreateSingleFileQueue(File.FileName);

				KeyValuePair<long, long> LastFileState = await File.Queue.GetFileState();

				return new Tuple<long, long, long, long>(
					FirstFileState.Key, FirstFileState.Value,
					LastFileState.Key, LastFileState.Value);
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		public Task<bool> Enqueue(object Item)
		{
			return this.Enqueue(Item, int.MaxValue);
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If item was enqueued</returns>
		public async Task<bool> Enqueue(object Item, int TimeoutMilliseconds)
		{
			bool Released = false;

			await this.semaphore.WaitAsync();
			try
			{
				if (this.disposed)
					return false;

				QueuedFile File = this.files.Last.Value;
				if (await File.Queue.Enqueue(Item, TimeoutMilliseconds))
					return true;

				string NewFileName = Path.Combine(this.folderName, Guid.NewGuid().ToString() + ".queue");

				File = new QueuedFile()
				{
					FileName = NewFileName,
					Queue = await this.CreateSingleFileQueue(NewFileName)
				};

				this.files.AddLast(File);
				this.fileCount++;

				Task<bool> PendingResult = File.Queue.Enqueue(Item, TimeoutMilliseconds);

				this.semaphore.Release();
				Released = true;

				return await PendingResult;
			}
			finally
			{
				if (!Released)
					this.semaphore.Release();
			}
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
		/// available before the timeout occurs, null is returned.
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
		/// available before the timeout occurs, null is returned.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="RemoveItem">If the item should be removed from the queue, if found.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		private async Task<object> Dequeue(int TimeoutMilliseconds, bool RemoveItem)
		{
			if (this.disposed)
				return null;

			bool Released = false;

			await this.semaphore.WaitAsync();
			try
			{
				if (this.disposed)
					return null;

				QueuedFile File = this.files.First.Value;

				while (this.fileCount > 1)
				{
					if (File.Queue is null)
					{
						try
						{
							File.Queue = await this.CreateSingleFileQueue(File.FileName);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
							this.files.RemoveFirst();
							this.fileCount--;
							continue;
						}
					}

					object Result = await File.Queue.Dequeue(0, RemoveItem);
					if (!(Result is null))
						return Result;

					await File.Queue.DisposeAsync();
					this.files.RemoveFirst();
					this.fileCount--;

					if (System.IO.File.Exists(File.FileName))
					{
						try
						{
							System.IO.File.Delete(File.FileName);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					File = this.files.First.Value;
				}

				File = this.files.First.Value;

				if (File.Queue is null)
				{
					try
					{
						File.Queue = await this.CreateSingleFileQueue(File.FileName);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return null;
					}
				}

				Task<object> PendingResult = File.Queue.Dequeue(TimeoutMilliseconds, RemoveItem);

				this.semaphore.Release();
				Released = true;

				return await PendingResult;
			}
			finally
			{
				if (!Released)
					this.semaphore.Release();
			}
		}

		/// <summary>
		/// Clears the queue.
		/// </summary>
		public async Task Clear()
		{
			await this.semaphore.WaitAsync();
			try
			{
				QueuedFile File = this.files.First.Value;

				while (this.fileCount > 1)
				{
					if (!(File.Queue is null))
					{
						await File.Queue.DisposeAsync();
						File.Queue = null;
					}

					this.files.RemoveFirst();
					this.fileCount--;

					if (System.IO.File.Exists(File.FileName))
					{
						try
						{
							System.IO.File.Delete(File.FileName);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					File = this.files.First.Value;
				}

				File = this.files.First.Value;

				if (File.Queue is null)
				{
					try
					{
						File.Queue = await this.CreateSingleFileQueue(File.FileName);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return;
					}
				}

				await File.Queue.Clear();
			}
			finally
			{
				this.semaphore.Release();
			}
		}

	}
}
