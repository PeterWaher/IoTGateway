using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Reads a character from the console.
	/// </summary>
	public class ConsoleInReadCharacter : WorkItem
	{
		private readonly TaskCompletionSource<int> result;

		/// <summary>
		/// Reads a character from the console.
		/// </summary>
		/// <param name="Result">Where the result will be returned.</param>
		public ConsoleInReadCharacter(TaskCompletionSource<int> Result)
		{
			this.result = Result;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			try
			{
				int Result = System.Console.In.Read();
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
