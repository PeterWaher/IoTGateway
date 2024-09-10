using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Writes a string to the console
	/// </summary>
	public class ConsoleErrorWriteString : WorkItem
	{
		private readonly string value;

		/// <summary>
		/// Writes a string to the console
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public ConsoleErrorWriteString(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			System.Console.Error.Write(this.value);
			return Task.CompletedTask;
		}
	}
}
