using System;
using System.Threading.Tasks;

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
		public override async Task Execute()
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
