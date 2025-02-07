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
			return this.Execute(CancellationToken.None, false);
		}

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public Task Execute(CancellationToken Cancel)
		{
			return this.Execute(Cancel, true);
		}

		/// <summary>
		/// Executes the operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected abstract Task Execute(CancellationToken Cancel, bool RegisterCancelToken);

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
			Cancel.Register(() => this.processed.TrySetResult(false));
			return this.processed.Task;
		}
	}
}
