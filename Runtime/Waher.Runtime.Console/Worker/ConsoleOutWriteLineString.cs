using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Writes a string to the console, appending a newline at the end.
	/// </summary>
	public class ConsoleOutWriteLineString : WorkItem
	{
		private readonly string value;

		/// <summary>
		/// Writes a string to the console, appending a newline at the end.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public ConsoleOutWriteLineString(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected override Task Execute(CancellationToken Cancel, bool RegisterCancelToken)
		{
			System.Console.Out.WriteLine(this.value);
			return Task.CompletedTask;
		}
	}
}
