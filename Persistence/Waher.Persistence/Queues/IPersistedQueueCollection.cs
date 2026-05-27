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
		/// <returns>Persisted queue object instance.</returns>
		Task<IPersistedQueue> GetQueue(IDatabaseProvider Provider, string QueueName);

		/// <summary>
		/// Gets an array of available queue names.
		/// </summary>
		/// <param name="Provider">Database provider.</param>
		/// <returns>Array of queue names.</returns>
		Task<string[]> GetQueues(IDatabaseProvider Provider);
	}
}
