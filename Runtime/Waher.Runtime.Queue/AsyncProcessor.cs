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
		private readonly CancellationTokenSource cancel = new CancellationTokenSource();
		private readonly Task[] processors;
		private AsyncQueue<T> queue = new AsyncQueue<T>();
		private bool terminating = false;
		private bool terminated = false;

		/// <summary>
		/// Processes work tasks, in an asynchronous manner.
		/// </summary>
		/// <param name="NrProcessors">Number of processors working in parallel.</param>
		public AsyncProcessor(int NrProcessors)
		{
			if (NrProcessors <= 0)
				throw new ArgumentException("Number of processors must be positive.", nameof(NrProcessors));

			List<Task> Processors = new List<Task>();

			while (NrProcessors-- > 0)
				Processors.Add(this.PerformWork());

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
			this.cancel.Cancel();

			await Task.WhenAll(this.processors);

			this.queue?.Dispose();
			this.queue = null;
			this.terminated = true;
		}

		/// <summary>
		/// Queues a work item for processing. No information is returned wether the item
		/// was forwarded or discarded.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		public void Queue(T Work)
		{
			if (!this.terminating)
				this.queue?.Queue(Work);
		}

		/// <summary>
		/// Forwards a work item for processing.
		/// </summary>
		/// <param name="Work">Item to process.</param>
		/// <returns>If item was forwarded for processing (true), or if it was discarded (false).</returns>
		public Task<bool> Forward(T Work)
		{
			if (!this.terminating)
				return this.queue?.Forward(Work) ?? Task.FromResult(false);
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
		/// Performs console operations.
		/// </summary>
		private async Task PerformWork()
		{
			try
			{
				CancellationToken Cancel = this.cancel.Token;
				T Item;
				Task<T> Task;

				while (!this.terminating)
				{
					Task = this.queue?.Wait(Cancel);
					if (Task is null)
						break;

					Item = await Task;
					if (Item is null)
						break;

					try
					{
						await Item.Execute(Cancel);
						Item.Processed(!this.cancel.IsCancellationRequested);
					}
					catch (Exception ex)
					{
						Item.Processed(false);
						Log.Exception(ex);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
