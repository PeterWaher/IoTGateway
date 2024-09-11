using System;
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
		private static AsyncQueue<WorkItem> queue = new AsyncQueue<WorkItem>();
		private static bool terminating = false;
		private static bool terminated = false;

		static ConsoleWorker()
		{
			Log.Terminating += Log_Terminating;
			PerformWork();
		}

		private static void Log_Terminating(object sender, System.EventArgs e)
		{
			Terminate();
			Log.Terminating -= Log_Terminating;
		}

		internal static void Terminate()
		{
			terminating = true;
			queue?.Dispose();
			queue = null;
		}

		/// <summary>
		/// Queues a work item.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		/// <returns>If item was forwarded for processing (true), or if it was discarded (false).</returns>
		public static Task<bool> Queue(WorkItem Work)
		{
			if (!terminating)
				return queue?.Add(Work) ?? Task.FromResult(false);
			else
				return Task.FromResult(false);
		}

		/// <summary>
		/// If the console worker is being terminated.
		/// </summary>
		public static bool Terminating => terminating;

		/// <summary>
		/// If the console worker has been terminated.
		/// </summary>
		public static bool Terminated => terminated;

		/// <summary>
		/// Performs console operations.
		/// </summary>
		private static async void PerformWork()
		{
			try
			{
				WorkItem Item;

				while (!((Item = await (queue?.Wait() ?? Task.FromResult<WorkItem>(null))) is null))
				{
					try
					{
						await Item.Execute();
						Item.Processed(true);
					}
					catch (Exception ex)
					{
						Item.Processed(false);
						ex = Log.UnnestException(ex);

						Event Event = new Event(Log.GetEventType(ex), ex, string.Empty, string.Empty, string.Empty, EventLevel.Minor,
							string.Empty, string.Empty);

						foreach (IEventSink Sink in Log.Sinks)
						{
							if (Sink.GetType().FullName.Contains(".Console"))
								Event.Avoid(Sink);
						}

						Log.Event(Event);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				terminated = true;
			}
		}
	}
}
