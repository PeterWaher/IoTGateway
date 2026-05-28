using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Inteface for collections of persisted queues matching a given database provider.
	/// </summary>
	public interface IPersistedQueueCollection : IProcessingSupport<IDatabaseProvider>
	{
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
		Task<IPersistedQueue> GetQueue(IDatabaseProvider Provider, string QueueName, 
			bool CanBeNull);

		/// <summary>
		/// Gets an array of available queue names.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <returns>Array of queue names.</returns>
		Task<string[]> GetQueues(IDatabaseProvider Provider);
	}
}
