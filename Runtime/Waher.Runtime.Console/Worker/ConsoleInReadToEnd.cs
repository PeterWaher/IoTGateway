using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Reads a line from the console.
	/// </summary>
	public class ConsoleInReadToEnd : WorkItem
	{
		private readonly TaskCompletionSource<string> result;

		/// <summary>
		/// Reads a line from the console.
		/// </summary>
		/// <param name="Result">Where the result will be returned.</param>
		public ConsoleInReadToEnd(TaskCompletionSource<string> Result)
		{
			this.result = Result;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public override async Task Execute(CancellationToken Cancel)
		{
			try
			{
				string Data = await System.Console.In.ReadToEndAsync();
				this.result.TrySetResult(Data);
			}
			catch (Exception ex)
			{
				this.result.TrySetException(ex);
			}
		}
	}
}
