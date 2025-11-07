using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Maintains a record of active processors.
	/// </summary>
	public static class AsyncProcessors
	{
		private static readonly LinkedList<IAsyncProcessor> activeProcessors = new LinkedList<IAsyncProcessor>();

		/// <summary>
		/// Registers an asynchronous processor.
		/// </summary>
		/// <param name="Processor"></param>
		/// <returns></returns>
		internal static LinkedListNode<IAsyncProcessor> RegisterProcessor(IAsyncProcessor Processor)
		{
			lock (activeProcessors)
			{
				return activeProcessors.AddLast(Processor);
			}
		}

		/// <summary>
		/// Unregisters an asynchronous processor.
		/// </summary>
		/// <param name="Node">Processor node returned during registration.</param>
		internal static void UnregisterProcessor(LinkedListNode<IAsyncProcessor> Node)
		{
			if (!(Node is null))
			{
				lock (activeProcessors)
				{
					activeProcessors.Remove(Node);
				}
			}
		}

		/// <summary>
		/// Gets an array of all active processors.
		/// </summary>
		/// <returns>Array of active processors.</returns>
		public static IAsyncProcessor[] GetActiveProcessors()
		{
			lock (activeProcessors)
			{
				IAsyncProcessor[] Result = new IAsyncProcessor[activeProcessors.Count];
				activeProcessors.CopyTo(Result, 0);
				return Result;
			}
		}

		/// <summary>
		/// If there are active processors.
		/// </summary>
		public static bool HasActiveProcessors
		{
			get
			{
				lock (activeProcessors)
				{
					return !(activeProcessors.First is null);
				}
			}
		}

		/// <summary>
		/// Closes all active processors for new items.
		/// </summary>
		public static Task CloseAllProcessorsForTermination()
		{
			return CloseAllProcessorsForTermination(false, 0);
		}

		/// <summary>
		/// Closes all active processors for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		public static Task CloseAllProcessorsForTermination(bool WaitForCompletion)
		{
			return CloseAllProcessorsForTermination(WaitForCompletion, int.MaxValue);
		}

		/// <summary>
		/// Closes all active processors for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		/// <param name="Timeout">Timeout, in milliseconds. If this time is reached,
		/// current workers will be cancelled.</param>
		public static async Task CloseAllProcessorsForTermination(bool WaitForCompletion, int Timeout)
		{
			IAsyncProcessor[] Processors = GetActiveProcessors();
			int i, c = Processors.Length;
			Task[] Tasks = new Task[c];

			for (i = 0; i < c; i++)
				Tasks[i] = Processors[i].CloseForTermination(WaitForCompletion, Timeout);

			if (WaitForCompletion)
				await Task.WhenAll(Tasks);
		}
	}
}
