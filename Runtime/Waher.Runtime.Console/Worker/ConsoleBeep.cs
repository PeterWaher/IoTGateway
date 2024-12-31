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
		/// <param name="RegisterCancelToken">If task can be cancelled.</param>
		protected override Task Execute(CancellationToken Cancel, bool RegisterCancelToken)
		{
			System.Console.Beep();
			return Task.CompletedTask;
		}
	}
}
