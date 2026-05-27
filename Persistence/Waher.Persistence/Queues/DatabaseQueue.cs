using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Filters;
using Waher.Runtime.Profiling;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Queue persisted into the database.
	/// </summary>
	public class DatabaseQueue : IPersistedQueue
	{
		private readonly LinkedList<TaskCompletionSource<object>> waitingDequeuers = new LinkedList<TaskCompletionSource<object>>();
		private readonly SemaphoreSlim semaphore;
		private readonly Profiler profiler;
		private readonly bool profiling;
		private readonly string queueName;
		private readonly ProfilerThread enqueueThread;
		private readonly ProfilerThread dequeueThread;
		private bool disposed = false;

		/// <summary>
		/// Queue persisted into the database.
		/// </summary>
		/// <param name="QueueName">Name of queue in database.</param>
		public DatabaseQueue(string QueueName)
			: this(QueueName, null)
		{
		}

		/// <summary>
		/// Queue persisted into the database.
		/// </summary>
		/// <param name="QueueName">Name of queue in database.</param>
		/// <param name="Profiler">Optional profiler.</param>
		public DatabaseQueue(string QueueName, Profiler Profiler)
		{
			this.queueName = QueueName;
			this.profiler = Profiler;
			this.profiling = !(this.profiler is null);
			this.semaphore = new SemaphoreSlim(1);

			if (this.profiling)
			{
				this.enqueueThread = this.profiler.CreateThread("Enqueue(" + this.queueName + ")", ProfilerThreadType.Binary);
				this.dequeueThread = this.profiler.CreateThread("Dequeue(" + this.queueName + ")", ProfilerThreadType.Binary);

				this.enqueueThread.Start();
				this.dequeueThread.Start();
			}
			else
			{
				this.enqueueThread = null;
				this.dequeueThread = null;
			}
		}

		/// <summary>
		/// Queue name.
		/// </summary>
		public string QueueName => this.queueName;

		/// <summary>
		/// Gets the number of dequeuers waiting for items to be queued.
		/// </summary>
		/// <returns>Number of dequeuers waiting for items.</returns>
		public async Task<int> GetNrDequeuers()
		{
			await this.semaphore.WaitAsync();
			try
			{
				return this.waitingDequeuers.Count;
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Gets the number of enqueuers waiting for space to be available to enqueue
		/// new items.
		/// </summary>
		/// <returns>Number of enqueuers waiting for space.</returns>
		public Task<int> GetNrEnqueuers() => Task.FromResult(0);

		/// <summary>
		/// Disposes the connection
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Disposes of the object, asynchronously.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				await this.semaphore.WaitAsync();

				foreach (TaskCompletionSource<object> T in this.waitingDequeuers)
					T.TrySetResult(null);

				this.waitingDequeuers.Clear();

				if (this.profiling)
				{
					this.enqueueThread.Stop();
					this.dequeueThread.Stop();
				}
			}
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <returns>If item was enqueued</returns>
		public Task<bool> Enqueue(object Item)
		{
			return this.Enqueue(Item, int.MaxValue);
		}

		/// <summary>
		/// Enqueues an item into the queue.
		/// </summary>
		/// <param name="Item">Item to enqueue</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If item was enqueued</returns>
		public async Task<bool> Enqueue(object Item, int TimeoutMilliseconds)
		{
			if (Item is null)
				throw new ArgumentNullException(nameof(Item));

			if (this.profiling)
				this.enqueueThread.High();

			DateTime StartUtc = DateTime.UtcNow;
			TaskCompletionSource<bool> Wait = null;
			bool Forwarded;

			do
			{
				if (this.disposed)
				{
					if (this.profiling)
						this.enqueueThread.Low();

					return false;
				}

				if (!(Wait is null))
				{
					int Milliseconds = (int)(TimeoutMilliseconds - DateTime.UtcNow.Subtract(StartUtc).TotalMilliseconds);
					if (Milliseconds < 0)
					{
						if (this.profiling)
							this.enqueueThread.Low();

						return false;
					}

					_ = Task.Delay(Milliseconds).ContinueWith((_) =>
					{
						Wait?.TrySetResult(false);
						return Task.CompletedTask;
					});

					if (!await Wait.Task || this.disposed)
					{
						if (this.profiling)
							this.enqueueThread.Low();

						return false;
					}

					Wait = null;
				}

				await this.semaphore.WaitAsync();
				try
				{
					LinkedListNode<TaskCompletionSource<object>> First = this.waitingDequeuers.First;

					if (First is null)
					{
						QueuedItem QueuedItem = new QueuedItem()
						{
							QueueName = this.queueName,
							CreatedUtc = DateTime.UtcNow,
							Content = Item
						};

						await Database.Insert(QueuedItem);
						return true;
					}
					else
					{
						this.waitingDequeuers.RemoveFirst();
						Forwarded = First.Value.TrySetResult(Item);
					}
				}
				finally
				{
					this.semaphore.Release();
				}
			}
			while (!Forwarded);

			if (this.profiling)
				this.enqueueThread.Low();

			return true;
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <returns>Dequeued item.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public Task<object> Dequeue()
		{
			return this.Dequeue(int.MaxValue, true);
		}


		/// <summary>
		/// Tries to dequeue an item from the queue, if one exists. If an item is not 
		/// available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		public Task<object> TryDequeue()
		{
			return this.Dequeue(0, true);
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		public Task<object> Dequeue(int TimeoutMilliseconds)
		{
			return this.Dequeue(TimeoutMilliseconds, true);
		}

		/// <summary>
		/// Returns the next item available to be dequeued, without dequeueing it.
		/// If an item is not available, null is returned.
		/// </summary>
		/// <returns>Dequeued item, or null if no item available.</returns>
		public Task<object> Peek()
		{
			return this.Dequeue(0, false);
		}

		/// <summary>
		/// Dequeue an item from the queue. The task will wait for an item to be dequeued,
		/// or, if the queue is empty, for an item to be enqueued. If an item is not 
		/// available before the timeout occurs, null is returned. If all items have been 
		/// dequeued, the file is cleared, to conserve disk space.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="RemoveItem">If the item should be removed from the queue, if found.</param>
		/// <returns>Dequeued item, or null if no item available within the allotted time.</returns>
		/// <exception cref="InvalidOperationException">If file has been corrupted.</exception>
		private async Task<object> Dequeue(int TimeoutMilliseconds, bool RemoveItem)
		{
			if (TimeoutMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(TimeoutMilliseconds), "Value must be non-negative.");

			if (this.disposed)
				return null;

			if (this.profiling)
				this.dequeueThread.High();

			bool Released = false;

			await this.semaphore.WaitAsync();
			try
			{
				if (RemoveItem)
				{
					IEnumerable<QueuedItem> Items = await Database.FindDelete<QueuedItem>(0, 1,
						new FilterFieldEqualTo("QueueName", this.queueName),
						"CreatedUtc");

					foreach (QueuedItem Item in Items)
					{
						if (this.profiling)
							this.dequeueThread.Low();

						return Item.Content;
					}
				}
				else
				{
					QueuedItem Item = await Database.FindFirstIgnoreRest<QueuedItem>(
						new FilterFieldEqualTo("QueueName", this.queueName),
						"CreatedUtc");

					if (!(Item is null))
					{
						if (this.profiling)
							this.dequeueThread.Low();

						return Item.Content;
					}
				}

				if (TimeoutMilliseconds == 0)
				{
					if (this.profiling)
						this.dequeueThread.Low();

					return null;
				}

				TaskCompletionSource<object> Waiter = new TaskCompletionSource<object>();
				LinkedListNode<TaskCompletionSource<object>> Node = this.waitingDequeuers.AddLast(Waiter);

				this.semaphore.Release();
				Released = true;

				_ = Task.Delay(TimeoutMilliseconds).ContinueWith(async (_) =>
				{
					try
					{
						await this.semaphore.WaitAsync();
						try
						{
							if (!(Node.List is null))
								this.waitingDequeuers.Remove(Node);
						}
						finally
						{
							this.semaphore.Release();
						}

						Waiter.TrySetResult(null);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				});

				object Result = await Waiter.Task;

				if (!RemoveItem)
				{
					QueuedItem QueuedItem = new QueuedItem()
					{
						QueueName = this.queueName,
						CreatedUtc = DateTime.UtcNow,
						Content = Result
					};

					await Database.Insert(QueuedItem);
				}

				if (this.profiling)
					this.dequeueThread.Low();

				return Result;
			}
			finally
			{
				if (!Released)
					this.semaphore.Release();
			}
		}

		/// <summary>
		/// Clears the queue.
		/// </summary>
		public async Task Clear()
		{
			await this.semaphore.WaitAsync();
			try
			{
				await Database.Delete<QueuedItem>(
					new FilterFieldEqualTo("QueueName", this.queueName),
					"CreatedUtc");
			}
			finally
			{
				this.semaphore.Release();
			}
		}

	}
}
