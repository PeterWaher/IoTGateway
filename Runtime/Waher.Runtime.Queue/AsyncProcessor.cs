using System;
using System.Collections.Generic;
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
		private readonly CancellationTokenSource[] cancelSources;
		private readonly Task[] processors;
		private readonly int nrProcessors;
		private AsyncQueue<T> queue = new AsyncQueue<T>();
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

			List<Task> Processors = new List<Task>();
			int i;

			this.nrProcessors = NrProcessors;
			this.cancelSources = new CancellationTokenSource[NrProcessors];
			for (i = 0; i < this.nrProcessors; i++)
				Processors.Add(this.PerformWork(i));

			this.processors = Processors.ToArray();
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

			for (int i = 0; i < this.nrProcessors; i++)
				this.cancelSources[i]?.Cancel();

			await Task.WhenAll(this.processors);

			this.queue?.Dispose();
			this.queue = null;
			this.terminated = true;
		}

		/// <summary>
		/// Closes the processor for new items, but continues to process queued items.
		/// </summary>
		public void CloseForTermination()
		{
			this.terminating = true;
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
					using (CancellationTokenSource Cancel = new CancellationTokenSource())
					{
						this.cancelSources[ProcessorIndex] = Cancel;
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

							this.idle = false;
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
						}
						finally
						{
							this.cancelSources[ProcessorIndex] = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
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
