using System;
using System.Threading.Tasks;

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
		public override async Task Execute()
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
