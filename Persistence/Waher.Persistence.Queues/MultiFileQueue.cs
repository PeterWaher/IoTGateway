using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Queue persisted into one or more files.
	/// </summary>
	public class MultiFileQueue : IPersistedQueue
	{
		private readonly LinkedList<QueuedFile> files = new LinkedList<QueuedFile>();
		private readonly SerializerCollection serializers;
		private readonly GetKeysMethod getKeys;
		private readonly FilesProvider provider;
		private readonly SemaphoreSlim semaphore;
		private readonly string folderName;
		private readonly int maxFileSize;
		private readonly bool encrypted = false;
		private int fileCount;
		private bool disposed = false;

		private MultiFileQueue(string FolderName, int MaxFileSize, bool Encrypted,
			SerializerCollection Serializers, GetKeysMethod GetKeys)
		{
			this.folderName = FolderName;
			this.maxFileSize = MaxFileSize;
			this.encrypted = Encrypted;
			this.serializers = Serializers;
			this.getKeys = GetKeys;
			this.provider = null;
			this.semaphore = new SemaphoreSlim(1);
		}

		private MultiFileQueue(string FolderName, int MaxFileSize, bool Encrypted,
			FilesProvider Provider)
		{
			this.folderName = FolderName;
			this.maxFileSize = MaxFileSize;
			this.encrypted = Encrypted;
			this.serializers = null;
			this.getKeys = null;
			this.provider = Provider;
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
		public static async Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, SerializerCollection Serializers, GetKeysMethod GetKeys)
		{
			MultiFileQueue Result = new MultiFileQueue(FolderName, MaxFileSize, Encrypted,
				Serializers, GetKeys);

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
		public static async Task<MultiFileQueue> Create(string FolderName, bool Encrypted,
			int MaxFileSize, FilesProvider Provider)
		{
			MultiFileQueue Result = new MultiFileQueue(FolderName, MaxFileSize, Encrypted,
				Provider);

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
				return await SingleFileQueue.Create(FileName, this.encrypted, this.maxFileSize,
					QueueThresholdMode.Ignore, this.serializers, this.getKeys);
			}
			else
			{
				return await SingleFileQueue.Create(FileName, this.encrypted, this.maxFileSize,
					QueueThresholdMode.Ignore, this.provider);
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
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		public async Task<bool> Enqueue(object Item)
		{
			await this.semaphore.WaitAsync();
			try
			{
				if (this.disposed)
					return false;

				QueuedFile File = this.files.Last.Value;
				if (await File.Queue.Enqueue(Item))
					return true;

				string NewFileName = Path.Combine(this.folderName, Guid.NewGuid().ToString() + ".queue");

				File = new QueuedFile()
				{
					FileName = NewFileName,
					Queue = await this.CreateSingleFileQueue(NewFileName)
				};

				this.files.AddLast(File);
				this.fileCount++;

				return await File.Queue.Enqueue(Item);
			}
			finally
			{
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
			return this.Dequeue(int.MaxValue);
		}


		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public async Task<object> Dequeue(int TimeoutMilliseconds)
		{
			await this.semaphore.WaitAsync();
			try
			{
				if (this.disposed)
					return null;

				QueuedFile File = this.files.First.Value;
				object Result;

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

					Result = await File.Queue.Dequeue(0);
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

				return await File.Queue.Dequeue(TimeoutMilliseconds);
			}
			finally
			{
				this.semaphore.Release();
			}
		}
	}
}
