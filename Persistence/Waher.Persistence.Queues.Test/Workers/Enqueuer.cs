using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using System;

#if !LW
namespace Waher.Persistence.Queues.Test.Workers
#else
using Waher.Persistence.Queues;
namespace Waher.Persistence.QueuesLW.Test.Workers
#endif
{
	internal class Enqueuer
	{
		private readonly TaskCompletionSource<bool> enqueueDone;
		private readonly IPersistedQueue queue;
		private readonly Random rnd;
		private readonly int minTime;
		private readonly int maxTime;
		private int start;
		private int nr;

		public Enqueuer(int Start, int Nr, int MinTime, int MaxTime, Random Rnd, IPersistedQueue Queue)
		{
			this.start = Start;
			this.nr = Nr;
			this.minTime = MinTime;
			this.maxTime = MaxTime;
			this.rnd = Rnd;
			this.queue = Queue;
			this.enqueueDone = new TaskCompletionSource<bool>();
		}

		public Task<bool> Wait() => this.enqueueDone.Task;

		public async Task Start()
		{
			try
			{
				while (this.nr-- > 0)
				{
					await Task.Delay(this.rnd.Next(this.minTime, this.maxTime), 
						CancellationToken.None);

					Assert.IsTrue(await this.queue.Enqueue(this.start++));
				}

				this.enqueueDone.TrySetResult(true);
			}
			catch (Exception ex)
			{
				this.enqueueDone.TrySetException(ex);
			}
		}
	}
}
