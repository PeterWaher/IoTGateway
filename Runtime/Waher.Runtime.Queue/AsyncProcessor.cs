using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Processes work tasks, in an asynchronous manner.
	/// </summary>
	public class AsyncProcessor<T> : IDisposableAsync
		where T : class, IWorkItem
	{
		private readonly CancellationTokenSource[] cancelWaitSources;
		private readonly CancellationTokenSource[] cancelWorkSources;
		private readonly Task[] processors;
		private readonly int nrProcessors;
		private AsyncQueue<T> queue = new AsyncQueue<T>();
		private int nrProcessorsRunning;
		private bool terminating = false;
		private bool terminated = false;
		private bool idle = true;

		/// <summary>
		/// Processes work tasks, in an asynchronous manner.
		/// </summary>
		/// <param name="NrProcessors">Number of processors working in parallel.</param>
		public AsyncProcessor(int NrProcessors)
		{
			if (NrProcessors <= 0)
				throw new ArgumentException("Number of processors must be positive.", nameof(NrProcessors));

			int i;

			this.nrProcessors = this.nrProcessorsRunning = NrProcessors;
			this.processors = new Task[NrProcessors];

			this.cancelWaitSources = new CancellationTokenSource[NrProcessors];
			this.cancelWorkSources = new CancellationTokenSource[NrProcessors];

			for (i = 0; i < this.nrProcessors; i++)
				this.processors[i] = this.PerformWork(i);
		}

		/// <summary>
		/// Stops processing tasks and disposes the queue.
		/// </summary>
		[Obsolete("Use the DisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Stops processing tasks and disposes the queue.
		/// </summary>
		public async Task DisposeAsync()
		{
			this.terminating = true;

			if (!this.terminated)
			{
				for (int i = 0; i < this.nrProcessors; i++)
				{
					this.cancelWaitSources[i]?.Cancel();
					this.cancelWorkSources[i]?.Cancel();
				}

				await Task.WhenAll(this.processors);
				this.terminated = true;
			}

			this.queue?.Dispose();
			this.queue = null;
		}

		/// <summary>
		/// Closes the processor for new items, but continues to process queued items.
		/// </summary>
		public void CloseForTermination()
		{
			this.terminating = true;

			for (int i = 0; i < this.nrProcessors; i++)
				this.cancelWaitSources[i]?.Cancel();
		}

		/// <summary>
		/// Closes the processor for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		public Task CloseForTermination(bool WaitForCompletion)
		{
			return this.CloseForTermination(WaitForCompletion, int.MaxValue);
		}

		/// <summary>
		/// Closes the processor for new items. If <paramref name="WaitForCompletion"/> is
		/// true, the method waits for queued items to be processed.
		/// </summary>
		/// <param name="WaitForCompletion">If the method should wait for queued items
		/// to be processed.</param>
		/// <param name="Timeout">Timeout, in milliseconds. If this time is reached,
		/// current workers will be cancelled.</param>
		public async Task CloseForTermination(bool WaitForCompletion, int Timeout)
		{
			this.terminating = true;

			for (int i = 0; i < this.nrProcessors; i++)
				this.cancelWaitSources[i]?.Cancel();

			if (Timeout > 0 && Timeout < int.MaxValue)
			{
				_ = Task.Delay(Timeout).ContinueWith((_) =>
				{
					for (int i = 0; i < this.nrProcessors; i++)
						this.cancelWorkSources[i]?.Cancel();
				});
			}

			if (!this.terminated)
			{
				await Task.WhenAll(this.processors);
				this.terminated = true;
			}
		}

		/// <summary>
		/// Queues a work item for processing. No information is returned wether the item
		/// was forwarded or discarded.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		public void Queue(T Work)
		{
			if (!this.terminating)
			{
				this.idle = false;
				this.queue?.Queue(Work);
			}
		}

		/// <summary>
		/// Forwards a work item for processing.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		/// <returns>If item was forwarded for processing (true), or if it was discarded (false).</returns>
		public Task<bool> Forward(T Work)
		{
			if (!this.terminating)
			{
				this.idle = false;
				return this.queue?.Forward(Work) ?? Task.FromResult(false);
			}
			else
				return Task.FromResult(false);
		}

		/// <summary>
		/// If the console worker is being terminated.
		/// </summary>
		public bool Terminating => this.terminating;

		/// <summary>
		/// If the console worker has been terminated.
		/// </summary>
		public bool Terminated => this.terminated;

		/// <summary>
		/// Number of items in queue.
		/// </summary>
		public int QueueSize => this.queue?.CountItems ?? 0;

		/// <summary>
		/// If processor is idle
		/// </summary>
		public bool Idle => this.idle;

		/// <summary>
		/// Performs console operations.
		/// </summary>
		/// <param name="ProcessorIndex">Index of processor.</param>
		private async Task PerformWork(int ProcessorIndex)
		{
			try
			{
				ProcessorEventArgs e = new ProcessorEventArgs(ProcessorIndex);
				T Item;
				Task<T> Task;

				while (true)
				{
					if (this.terminating)
					{
						if (!(this.queue?.TryGetItem(out Item) ?? false))
							break;
					}
					else
					{
						using (CancellationTokenSource Cancel = new CancellationTokenSource())
						{
							this.cancelWaitSources[ProcessorIndex] = Cancel;
							try
							{
								Task = this.queue?.Wait(Cancel.Token);
								if (Task is null)
									break;

								if (!Task.IsCompleted && this.queue.CountSubscribers == this.nrProcessors)
								{
									this.idle = true;
									await this.OnIdle.Raise(this, e);
								}

								Item = await Task;
								if (Item is null)
									break;
							}
							finally
							{
								this.cancelWaitSources[ProcessorIndex] = null;
							}
						}
					}

					this.idle = false;

					using (CancellationTokenSource Cancel = new CancellationTokenSource())
					{
						this.cancelWorkSources[ProcessorIndex] = Cancel;
						try
						{
							await Item.Execute(Cancel.Token);
							Item.Processed(!Cancel.IsCancellationRequested);
						}
						catch (Exception ex)
						{
							Item.Processed(false);
							Log.Exception(ex);
						}
						finally
						{
							this.cancelWorkSources[ProcessorIndex] = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			if (--this.nrProcessorsRunning == 0)
				this.terminated = true;
		}

		/// <summary>
		/// Event raised when the processor becomes idle. It can be raised multiple
		/// times if more than one processor is used, once for each processor becoming idle.
		/// </summary>
		public event EventHandlerAsync<ProcessorEventArgs> OnIdle;

		/// <summary>
		/// Waits until the processor becomes idle.
		/// </summary>
		public async Task WaitUntilIdle()
		{
			TaskCompletionSource<bool> Idle = new TaskCompletionSource<bool>();

			Task OnIdle(object Sender, EventArgs e)
			{
				Task.Run(() => Idle.TrySetResult(true));
				return Task.CompletedTask;
			}
			;

			this.OnIdle += OnIdle;
			try
			{
				if (this.idle)
					return;

				await Idle.Task;
			}
			finally
			{
				this.OnIdle -= OnIdle;
			}
		}
	}
}
