using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Queue
{
	/// <summary>
	/// Asynchronous First-in-First-out (FIFO) Queue, for use when transporting items 
	/// of type <typeparamref name="T"/> between tasks.
	/// </summary>
	/// <typeparam name="T">Items of this class is managed by the queue.</typeparam>
	public class AsyncQueue<T> : IDisposable
		where T : class
	{
		private readonly LinkedList<Item> queue = new LinkedList<Item>();
		private readonly LinkedList<TaskCompletionSource<T>> subscribers = new LinkedList<TaskCompletionSource<T>>();
		private readonly TaskCompletionSource<bool> terminatedTask = new TaskCompletionSource<bool>();
		private readonly object synchObj = new object();
		private volatile int countItems = 0;
		private volatile int countSubscribers = 0;
		private bool disposed = false;
		private bool terminated = false;

		/// <summary>
		/// Asynchronous Queue, for use when transporting items of class 
		/// <typeparamref name="T"/> between tasks.
		/// </summary>
		public AsyncQueue()
		{
		}

		private class Item
		{
			internal T Value { get; }
			internal TaskCompletionSource<bool> Processed = new TaskCompletionSource<bool>();

			internal Item(T Value)
			{
				this.Value = Value;
			}
		}

		/// <summary>
		/// Number of items in queue.
		/// </summary>
		public int CountItems
		{
			get
			{
				lock (this.synchObj)
				{
					return this.countItems;
				}
			}
		}

		/// <summary>
		/// Number of subscribers waiting for items.
		/// </summary>
		public int CountSubscribers
		{
			get
			{
				lock (this.synchObj)
				{
					return this.countSubscribers;
				}
			}
		}

		/// <summary>
		/// Adds an item last to the queue.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		/// <returns>If item was forwarded for processing (true), or discarded (false).</returns>
		public Task<bool> Add(T Item)
		{
			return this.Add(Item, false);
		}

		/// <summary>
		/// Adds an item last to the queue.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		/// <returns>If item was forwarded for processing (true), or discarded (false).</returns>
		public Task<bool> AddLast(T Item)
		{
			return this.Add(Item, false);
		}

		/// <summary>
		/// Adds an item first to the queue.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		/// <returns>If item was forwarded for processing (true), or discarded (false).</returns>
		public Task<bool> AddFirst(T Item)
		{
			return this.Add(Item, true);
		}

		/// <summary>
		/// Adds an item to the queue.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		/// <param name="First">If item is to be added first in the queue.</param>
		/// <returns>If item was forwarded for processing (true), or discarded (false).</returns>
		public Task<bool> Add(T Item, bool First)
		{
			lock (this.synchObj)
			{
				if (this.terminated || this.disposed)
					throw new ObjectDisposedException("Queue has been terminated.");

				if (this.subscribers.First is null)
				{
					Item Record = new Item(Item);

					if (First)
						this.queue.AddFirst(Record);
					else
						this.queue.AddLast(Record);

					this.countItems++;

					return Record.Processed.Task;
				}
				else
				{
					TaskCompletionSource<T> Waiter = this.subscribers.First.Value;
					this.subscribers.RemoveFirst();
					this.countSubscribers--;
					Waiter.TrySetResult(Item);

					return Task.FromResult(true);
				}
			}
		}

		/// <summary>
		/// Waits indefinitely (or until queue is disposed) for an item to be available.
		/// If Queue is disposed, a null item will be returned to the subscriber.
		/// </summary>
		/// <returns>Item to process, or null if queue is disposed.</returns>
		public Task<T> Wait()
		{
			Item Record;

			lock (this.synchObj)
			{
				if (this.disposed)
					return Task.FromResult<T>(null);

				if (this.queue.First is null)
				{
					TaskCompletionSource<T> Task = new TaskCompletionSource<T>();
					this.subscribers.AddLast(Task);
					this.countSubscribers++;
					return Task.Task;
				}
				else
				{
					Record = this.queue.First.Value;
					this.queue.RemoveFirst();
					this.countItems--;

					Record.Processed.TrySetResult(true);

					if (this.terminated && this.queue.First is null)
						this.disposed = true;
					else
						return Task.FromResult(Record.Value);
				}
			}

			this.RaiseDisposed();

			return Task.FromResult(Record.Value);
		}

		/// <summary>
		/// Tries to get a queued item, if found. If not, the method returns
		/// immediately with a null item.
		/// </summary>
		/// <param name="Item">Item, if found, null otherwise.</param>
		/// <returns>If an item was found, and returned.</returns>
		public bool TryPeekItem(out T Item)
		{
			return this.TryGetItem(false, out Item);
		}

		/// <summary>
		/// Tries to get a queued item, if found. If not, the method returns
		/// immediately with a null item.
		/// </summary>
		/// <param name="Item">Item, if found, null otherwise.</param>
		/// <returns>If an item was found, and returned.</returns>
		public bool TryGetItem(out T Item)
		{
			return this.TryGetItem(true, out Item);
		}

		private bool TryGetItem(bool Remove, out T Item)
		{
			lock (this.synchObj)
			{
				if (this.disposed || this.queue.First is null)
				{
					Item = null;
					return false;
				}
				else
				{
					Item Record = this.queue.First.Value;
					Item = Record.Value;

					if (Remove)
					{
						this.queue.RemoveFirst();
						this.countItems--;

						Record.Processed.TrySetResult(true);

						if (this.terminated && this.queue.First is null)
							this.disposed = true;
						else
							return true;
					}
					else
						return true;
				}
			}

			this.RaiseDisposed();
			return true;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			lock (this.synchObj)
			{
				if (this.disposed)
					return;

				this.disposed = true;
				this.terminated = true;

				foreach (Item Record in this.queue)
					Record.Processed.TrySetResult(false);

				this.queue.Clear();
				this.countItems = 0;

				foreach (TaskCompletionSource<T> Task in this.subscribers)
					Task.TrySetResult(null);

				this.subscribers.Clear();
				this.countSubscribers = 0;
			}

			this.RaiseDisposed();
		}

		/// <summary>
		/// Terminates the queue, allowing subscribers to get queued items, but
		/// disallows new items to be added. When all items have been processed,
		/// queue is disposed, and the returning Task will be completed.
		/// </summary>
		public Task Terminate()
		{
			lock (this.synchObj)
			{
				this.terminated = true;

				if (this.queue.First is null)
				{
					this.disposed = true;

					foreach (Item Record in this.queue)
						Record.Processed.TrySetResult(false);

					this.queue.Clear();
					this.countItems = 0;

					foreach (TaskCompletionSource<T> Task in this.subscribers)
						Task.TrySetResult(null);

					this.subscribers.Clear();
					this.countSubscribers = 0;
				}
				else
					return this.terminatedTask.Task;
			}

			this.RaiseDisposed();
			return this.terminatedTask.Task;
		}

		/// <summary>
		/// Event raised when queue has been disposed.
		/// </summary>
		public event EventHandler Disposed = null;

		private void RaiseDisposed()
		{
			this.terminatedTask.TrySetResult(true);

			try
			{
				EventHandler h = this.Disposed;

				if (!(h is null))
					h(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

	}
}
