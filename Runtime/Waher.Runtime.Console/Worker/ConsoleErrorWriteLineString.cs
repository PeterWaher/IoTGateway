using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Writes a string to the console, appending a newline at the end.
	/// </summary>
	public class ConsoleErrorWriteLineString : WorkItem
	{
		private readonly string value;

		/// <summary>
		/// Writes a string to the console, appending a newline at the end.
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public ConsoleErrorWriteLineString(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			System.Console.Error.WriteLine(this.value);
			return Task.CompletedTask;
		}
	}
}
