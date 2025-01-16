using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Interface for asynchronous operations.
	/// </summary>
	public interface IWorkItem
	{
		/// <summary>
		/// Executes the operation.
		/// </summary>
		Task Execute();

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		Task Execute(CancellationToken Cancel);

		/// <summary>
		/// Flags the item as processed.
		/// </summary>
		/// <returns>If item was processed successfully.</returns>
		void Processed(bool Result);

		/// <summary>
		/// Waits for the item to be processed.
		/// </summary>
		/// <returns>If item was processed successfully.</returns>
		Task<bool> Wait();

		/// <summary>
		/// Waits for the item to be processed.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <returns>If item was processed successfully.</returns>
		Task<bool> Wait(CancellationToken Cancel);
	}
}
