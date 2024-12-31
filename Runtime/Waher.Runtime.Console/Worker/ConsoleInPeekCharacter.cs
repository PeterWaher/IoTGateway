using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Peeks a character from the console.
	/// </summary>
	public class ConsoleInPeekCharacter : WorkItem
	{
		private readonly TaskCompletionSource<int> result;

		/// <summary>
		/// Peeks a character from the console.
		/// </summary>
		/// <param name="Result">Where the result will be returned.</param>
		public ConsoleInPeekCharacter(TaskCompletionSource<int> Result)
		{
			this.result = Result;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected override Task Execute(CancellationToken Cancel, bool RegisterCancelToken)
		{
			try
			{
				int Result = System.Console.In.Peek();
				this.result.TrySetResult(Result);
			}
			catch (Exception ex)
			{
				this.result.TrySetException(ex);
			}

			return Task.CompletedTask;
		}
	}
}
