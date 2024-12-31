using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Reads a line from the console.
	/// </summary>
	public class ConsoleInReadLine : WorkItem
	{
		private readonly TaskCompletionSource<string> result;

		/// <summary>
		/// Reads a line from the console.
		/// </summary>
		/// <param name="Result">Where the result will be returned.</param>
		public ConsoleInReadLine(TaskCompletionSource<string> Result)
		{
			this.result = Result;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected override async Task Execute(CancellationToken Cancel, bool RegisterCancelToken)
		{
			try
			{
				string Row = await System.Console.In.ReadLineAsync();
				this.result.TrySetResult(Row);
			}
			catch (Exception ex)
			{
				this.result.TrySetException(ex);
			}
		}
	}
}
