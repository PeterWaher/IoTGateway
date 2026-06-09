using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Collection of database-based persisted queues.
	/// </summary>
	public class DatabaseQueueCollection : IPersistedQueueCollection
	{
		private static readonly Dictionary<string, IPersistedQueue> queues = new Dictionary<string, IPersistedQueue>();

		/// <summary>
		/// Collection of database-based persisted queues.
		/// </summary>
		public DatabaseQueueCollection()
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
			return Grade.Barely;	// Slow compared to more efficient queues, so it should only be used if no other is found.
		}

		/// <summary>
		/// Gets a persisted queue with the specified name. If one is not found, a new one
		/// is created.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <param name="QueueName">Name of the queue.</param>
		/// <param name="CanBeNull">Can return null if a queue is not found.
		/// If null cannot be returned, and a queue with the given name is not found,
		/// a queue with the given name is created. (This is the default option.)</param>
		/// <returns>Persisted queue object instance, or null if no queue with the given name
		/// has been previously been created, and null can be returned.</returns>
		public Task<IPersistedQueue> GetQueue(IDatabaseProvider Provider, string QueueName, 
			bool CanBeNull)
		{
			lock (queues)
			{
				if (!queues.TryGetValue(QueueName, out IPersistedQueue Result))
				{
					Result = new DatabaseQueue(QueueName);
					queues[QueueName] = Result;
				}

				return Task.FromResult(Result);
			}
		}

		/// <summary>
		/// Gets an array of available queue names.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <returns>Array of queue names.</returns>
		public Task<string[]> GetQueues(IDatabaseProvider Provider)
		{
			lock (queues)
			{
				string[] Result = new string[queues.Count];
				queues.Keys.CopyTo(Result, 0);
				return Task.FromResult(Result);
			}
		}
	}
}
