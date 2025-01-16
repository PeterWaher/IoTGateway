using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Reads a key from the console.
	/// </summary>
	public class ConsoleInReadKey : WorkItem
	{
		private readonly TaskCompletionSource<ConsoleKeyInfo> result;
		private readonly bool intercept;

		/// <summary>
		/// Reads a key from the console.
		/// </summary>
		/// <param name="Intercept">Intercept</param>
		/// <param name="Result">Where the result will be returned.</param>
		public ConsoleInReadKey(bool Intercept, TaskCompletionSource<ConsoleKeyInfo> Result)
		{
			this.result = Result;
			this.intercept = Intercept;
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
				ConsoleKeyInfo Result = System.Console.ReadKey(this.intercept);
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
