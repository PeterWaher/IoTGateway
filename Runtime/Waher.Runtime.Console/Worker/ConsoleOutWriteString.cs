using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Writes a string to the console
	/// </summary>
	public class ConsoleOutWriteString : WorkItem
	{
		private readonly string value;

		/// <summary>
		/// Writes a string to the console
		/// </summary>
		/// <param name="Value">Value to write.</param>
		public ConsoleOutWriteString(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			System.Console.Out.Write(this.value);
			return Task.CompletedTask;
		}
	}
}
