using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Emits a Beep sounds.
	/// </summary>
	public class ConsoleBeep : WorkItem
	{
		/// <summary>
		/// Emits a Beep sounds.
		/// </summary>
		public ConsoleBeep()
		{
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			System.Console.Beep();
			return Task.CompletedTask;
		}
	}
}
