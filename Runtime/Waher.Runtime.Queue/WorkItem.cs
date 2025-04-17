using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Represents an asynchronous operation to be performed.
	/// </summary>
	public abstract class WorkItem : IWorkItem
	{
		private readonly TaskCompletionSource<bool> processed = new TaskCompletionSource<bool>();

		/// <summary>
		/// Executes the operation.
		/// </summary>
		public Task Execute()
		{
			return this.Execute(CancellationToken.None);
		}

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public abstract Task Execute(CancellationToken Cancel);

		/// <summary>
		/// Flags the item as processed.
		/// </summary>
		/// <returns>If item was processed successfully.</returns>
		public void Processed(bool Result)
		{
			this.processed.TrySetResult(Result);
		}

		/// <summary>
		/// Waits for the item to be processed.
		/// </summary>
		/// <returns>If item was processed successfully.</returns>
		public Task<bool> Wait()
		{
			return this.processed.Task;
		}

		/// <summary>
		/// Waits for the item to be processed.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <returns>If item was processed successfully.</returns>
		public Task<bool> Wait(CancellationToken Cancel)
		{
			if (Cancel.CanBeCanceled)
				Cancel.Register(() => this.processed.TrySetResult(false));

			return this.processed.Task;
		}
	}
}
