using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

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
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Execute(CancellationToken Cancel)
		{
			System.Console.Beep();
			return Task.CompletedTask;
		}
	}
}
