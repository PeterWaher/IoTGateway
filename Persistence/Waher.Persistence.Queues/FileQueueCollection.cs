using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Manages a collection of file-based persisted queues integrated with the
	/// <see cref="FilesProvider"/> database provider.
	/// </summary>
	public class FileQueueCollection : IPersistedQueueCollection
	{
		private static readonly Dictionary<string, IPersistedQueue> queues = new Dictionary<string, IPersistedQueue>();
		private static readonly SemaphoreSlim queuesLock = new SemaphoreSlim(1);
		private static FullSerialization fullSerialization = null;
		private const int maxFileSize = 10 * 1024 * 1024;   // Max 10 MB per queue file.

		/// <summary>
		/// Manages a collection of file-based persisted queues integrated with the
		/// <see cref="FilesProvider"/> database provider.
		/// </summary>
		public FileQueueCollection()
		{
		}

		/// <summary>
		/// How well the database-based queue collection supports the specified database 
		/// provider.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <returns>Support grade.</returns>
		public Grade Supports(IDatabaseProvider Provider)
		{
			if (Provider is FilesProvider)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Gets a persisted queue with the specified name. If one is not found, a new one
		/// is created.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <param name="QueueName">Name of the queue.</param>
		/// <returns>Persisted queue object instance.</returns>
		public async Task<IPersistedQueue> GetQueue(IDatabaseProvider Provider, string QueueName)
		{
			if (!(Provider is FilesProvider FilesProvider))
				throw new ArgumentException("The specified database provider is not supported by this queue collection.", nameof(Provider));

			string FolderName = Path.Combine(FilesProvider.Folder, "Queues", QueueName);

			await queuesLock.WaitAsync();
			try
			{
				if (queues.TryGetValue(FolderName, out IPersistedQueue Result))
					return Result;

				if (fullSerialization is null)
					fullSerialization = new FullSerialization();

				Result = await MultiFileQueue.Create(FolderName, true, maxFileSize,
					fullSerialization, FilesProvider);

				queues[FolderName] = Result;

				return Result;
			}
			finally
			{
				queuesLock.Release();
			}
		}

		/// <summary>
		/// Gets an array of available queue names.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <returns>Array of queue names.</returns>
		public Task<string[]> GetQueues(IDatabaseProvider Provider)
		{
			if (!(Provider is FilesProvider FilesProvider))
				throw new ArgumentException("The specified database provider is not supported by this queue collection.", nameof(Provider));

			string FolderName = Path.Combine(FilesProvider.Folder, "Queues");
			if (!Directory.Exists(FolderName))
				return Task.FromResult(Array.Empty<string>());

			string[] SubDirectories = Directory.GetDirectories(FolderName, "*.*", SearchOption.TopDirectoryOnly);
			int i, c = SubDirectories.Length;

			for (i = 0; i < c; i++)
				SubDirectories[i] = Path.GetFileName(SubDirectories[i]);

			return Task.FromResult(SubDirectories);
		}
	}
}
