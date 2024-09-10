using System;
using System.Threading.Tasks;

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
		public override Task Execute()
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
