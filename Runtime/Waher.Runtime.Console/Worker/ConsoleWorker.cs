using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Processes console tasks, such as input and output, in a serialized asynchronous manner.
	/// </summary>
	public static class ConsoleWorker
	{
		private static AsyncProcessor<WorkItem> worker = new AsyncProcessor<WorkItem>(1);

		static ConsoleWorker()
		{
			Log.Terminating += Log_Terminating;
		}

		private static async Task Log_Terminating(object Sender, System.EventArgs e)
		{
			await Terminate();
			Log.Terminating -= Log_Terminating;
		}

		internal static async Task Terminate()
		{
			if (!(worker is null))
			{
				await worker.DisposeAsync();
				worker = null;
			}
		}

		/// <summary>
		/// Forwards a work item for processing.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		/// <returns>If item was forwarded for processing (true), or if it was discarded (false).</returns>
		public static Task<bool> Forward(WorkItem Work)
		{
			return worker?.Forward(Work) ?? Task.FromResult(false);
		}

		/// <summary>
		/// If the console worker is being terminated.
		/// </summary>
		public static bool Terminating => worker?.Terminating ?? true;

		/// <summary>
		/// If the console worker has been terminated.
		/// </summary>
		public static bool Terminated => worker?.Terminated ?? true;
	}
}
