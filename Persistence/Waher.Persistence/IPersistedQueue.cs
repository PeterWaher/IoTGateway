using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Persistence
{
	/// <summary>
	/// Inteface for persisted queues.
	/// </summary>
	public interface IPersistedQueue : IDisposableAsync
	{
		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		Task<bool> Enqueue(object Item);

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If item was enqueued</returns>
		Task<bool> Enqueue(object Item, int TimeoutMilliseconds);

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <returns>Dequeued item.</returns>
		Task<object> Dequeue();

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		Task<object> Dequeue(int TimeoutMilliseconds);

		/// <summary>
		/// Tries to dequeue an item from the queue, if one exists. If an item is not 
		/// available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		Task<object> TryDequeue();

		/// <summary>
		/// Returns the next item available to be dequeued, without dequeueing it.
		/// If an item is not available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		Task<object> Peek();

		/// <summary>
		/// Gets the number of dequeuers waiting for items to be queued.
		/// </summary>
		/// <returns>Number of dequeuers waiting for items.</returns>
		Task<int> GetNrDequeuers();

		/// <summary>
		/// Gets the number of enqueuers waiting for space to be available to enqueue
		/// new items.
		/// </summary>
		/// <returns>Number of enqueuers waiting for space.</returns>
		Task<int> GetNrEnqueuers();
	}
}
