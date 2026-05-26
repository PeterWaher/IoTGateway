using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

#if !LW
namespace Waher.Persistence.Queues.Test.Workers
#else
using Waher.Persistence.Queues;
namespace Waher.Persistence.QueuesLW.Test.Workers
#endif
{
	internal class Dequeuer
	{
		private readonly TaskCompletionSource<bool> dequeueDone;
		private readonly SortedDictionary<int, bool> dequeued;
		private readonly IPersistedQueue queue;
		private readonly Random rnd;
		private readonly int minTime;
		private readonly int maxTime;
		private int nr;

		public Dequeuer(int Nr, int MinTime, int MaxTime, Random Rnd, IPersistedQueue Queue,
			SortedDictionary<int, bool> Dequeued)
		{
			this.nr = Nr;
			this.minTime = MinTime;
			this.maxTime = MaxTime;
			this.rnd = Rnd;
			this.queue = Queue;
			this.dequeued = Dequeued;
			this.dequeueDone = new TaskCompletionSource<bool>();
		}

		public Task<bool> Wait() => this.dequeueDone.Task;

		public async Task Start()
		{
			try
			{
				while (this.nr-- > 0)
				{
					await Task.Delay(this.rnd.Next(this.minTime, this.maxTime), CancellationToken.None);

					object Item = await this.queue.Dequeue(10000);

					if (Item is not int i)
						throw new Exception("Invalid item dequeued.");

					lock (this.dequeued)
					{
						this.dequeued[i] = true;
					}
				}

				this.dequeueDone.TrySetResult(true);
			}
			catch (Exception ex)
			{
				this.dequeueDone.TrySetException(ex);
			}
		}
	}
}
