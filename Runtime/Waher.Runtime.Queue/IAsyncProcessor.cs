using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Interface for <see cref="AsyncProcessor{T}"/> instances.
	/// </summary>
	public interface IAsyncProcessor : IDisposableAsync
	{
		/// <summary>
		/// Closes the processor for new items, but continues to process queued items.
		/// </summary>
		void CloseForTermination();

		/// <summary>
		/// Closes the processor for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		Task CloseForTermination(bool WaitForCompletion);

		/// <summary>
		/// Closes the processor for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		/// <param name="Timeout">Timeout, in milliseconds. If this time is reached,
		/// current workers will be cancelled.</param>
		Task CloseForTermination(bool WaitForCompletion, int Timeout);

		/// <summary>
		/// Name of processor.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// If the console worker is being terminated.
		/// </summary>
		bool Terminating { get; }

		/// <summary>
		/// If the console worker has been terminated.
		/// </summary>
		bool Terminated { get; }

		/// <summary>
		/// Number of items in queue.
		/// </summary>
		int QueueSize { get; }

		/// <summary>
		/// If processor is idle
		/// </summary>
		bool Idle { get; }

		/// <summary>
		/// Event raised when the processor becomes idle. It can be raised multiple
		/// times if more than one processor is used, once for each processor becoming idle.
		/// </summary>
		event EventHandlerAsync<ProcessorEventArgs> OnIdle;

		/// <summary>
		/// Waits until the processor becomes idle.
		/// </summary>
		Task WaitUntilIdle();
	}
}
