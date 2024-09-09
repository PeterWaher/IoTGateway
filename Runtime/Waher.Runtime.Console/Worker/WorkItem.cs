using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Manages a Console operation.
	/// </summary>
	public abstract class WorkItem
	{
		private readonly TaskCompletionSource<bool> processed = new TaskCompletionSource<bool>();

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public abstract Task Execute();

		/// <summary>
		/// Flags the item as processed.
		/// </summary>
		/// <param name="Result">If item was forwarded for processing (true), or if it was discarded (false).</param>
		public void Processed(bool Result)
		{
			this.processed.TrySetResult(Result);
		}

		/// <summary>
		/// Waits for the item to be processed.
		/// </summary>
		/// <returns>If item was forwarded for processing (true), or if it was discarded (false).</returns>
		public Task<bool> Wait()
		{
			return this.processed.Task;
		}
	}
}
