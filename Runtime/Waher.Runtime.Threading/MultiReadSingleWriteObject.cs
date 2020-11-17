using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading
{
	/// <summary>
	/// Represents an object that allows single concurrent writers but multiple concurrent readers.
	/// When disposing the object, it automatically ends any reading and writing locks it maintains.
	/// </summary>
	public class MultiReadSingleWriteObject : IMultiReadSingleWriteObject, IDisposable
	{
		private LinkedList<TaskCompletionSource<bool>> noWriters = new LinkedList<TaskCompletionSource<bool>>();
		private LinkedList<TaskCompletionSource<bool>> noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
		private readonly object synchObj = new object();
		private int nrReaders = 0;
		private bool isWriting = false;

		/// <summary>
		/// Number of concurrent readers.
		/// </summary>
		public int NrReaders
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrReaders;
				}
			}
		}

		/// <summary>
		/// If the object is in a reading state.
		/// </summary>
		public bool IsReading
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrReaders > 0;
				}
			}
		}

		/// <summary>
		/// If the object has a writer.
		/// </summary>
		public bool IsWriting
		{
			get
			{
				lock (this.synchObj)
				{
					return this.isWriting;
				}
			}
		}

		/// <summary>
		/// If the object is locked for reading or writing.
		/// </summary>
		public bool IsReadingOrWriting
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrReaders > 0 || this.isWriting;
				}
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the object
		/// is not in a reading state.
		/// </summary>
		public void AssertReading()
		{
			lock (this.synchObj)
			{
				if (this.nrReaders <= 0)
					throw new InvalidOperationException("Not in a reading state.");
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the object
		/// is not in a writing state.
		/// </summary>
		public void AssertWriting()
		{
			lock (this.synchObj)
			{
				if (!this.isWriting)
					throw new InvalidOperationException("Not in a writing state.");
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the object
		/// is not in a reading or writing state.
		/// </summary>
		public void AssertReadingOrWriting()
		{
			lock (this.synchObj)
			{
				if (this.nrReaders <= 0 && !this.isWriting)
					throw new InvalidOperationException("Not in a reading or writing state.");
			}
		}

		/// <summary>
		/// Waits until object ready for reading.
		/// Each call to <see cref="BeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		public virtual async Task BeginRead()
		{
			TaskCompletionSource<bool> Wait = null;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!this.isWriting)
					{
						this.nrReaders++;
						return;
					}
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noWriters.AddLast(Wait);
					}
				}

				await Wait.Task;
			}
		}

		/// <summary>
		/// Ends a reading session of the object.
		/// Must be called once for each call to <see cref="BeginRead"/> or successful call to <see cref="TryBeginRead(int)"/>.
		/// </summary>
		public virtual Task EndRead()
		{
			LinkedList<TaskCompletionSource<bool>> List = null;

			lock (this.synchObj)
			{
				if (this.nrReaders <= 0)
					throw new InvalidOperationException("Not in a reading state.");

				this.nrReaders--;
				if (this.nrReaders > 0 || this.noReadersOrWriters.First is null)
					return Task.CompletedTask;

				List = this.noReadersOrWriters;
				this.noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
			}

			foreach (TaskCompletionSource<bool> T in List)
				T.TrySetResult(true);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for reading.
		/// Each successful call to <see cref="TryBeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public virtual async Task<bool> TryBeginRead(int Timeout)
		{
			TaskCompletionSource<bool> Wait = null;
			DateTime Start = DateTime.Now;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!this.isWriting)
					{
						this.nrReaders++;
						return true;
					}
					else if (Timeout <= 0)
						return false;
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noWriters.AddLast(Wait);
					}
				}

				using (Timer Timer = new Timer((P) =>
				{
					Wait.TrySetResult(false);

				}, null, Timeout, System.Threading.Timeout.Infinite))
				{
					DateTime Now = DateTime.Now;
					bool Result = await Wait.Task;
					if (!Result)
					{
						lock (this.synchObj)
						{
							this.noWriters.Remove(Wait);
						}

						return false;
					}

					Timeout -= (int)((Now - Start).TotalMilliseconds + 0.5);
					Start = Now;
				}
			}
		}

		/// <summary>
		/// Waits until object ready for writing.
		/// Each call to <see cref="BeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		public virtual async Task BeginWrite()
		{
			TaskCompletionSource<bool> Prev = null;
			TaskCompletionSource<bool> Wait = null;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!(Prev is null))
						this.noWriters.Remove(Prev);    // In case previously locked for reading

					if (this.nrReaders == 0 && !this.isWriting)
					{
						this.isWriting = true;
						return;
					}
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noReadersOrWriters.AddLast(Wait);
						this.noWriters.AddLast(Wait);
					}
				}

				await Wait.Task;
				Prev = Wait;
			}
		}

		/// <summary>
		/// Ends a writing session of the object.
		/// Must be called once for each call to <see cref="BeginWrite"/> or successful call to <see cref="TryBeginWrite(int)"/>.
		/// </summary>
		public virtual Task EndWrite()
		{
			LinkedList<TaskCompletionSource<bool>> List = null;
			LinkedList<TaskCompletionSource<bool>> List2 = null;

			lock (this.synchObj)
			{
				if (!this.isWriting)
					throw new InvalidOperationException("Not in a writing state.");

				this.isWriting = false;

				if (!(this.noReadersOrWriters.First is null))
				{
					List = this.noReadersOrWriters;
					this.noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
				}

				if (!(this.noWriters.First is null))
				{
					List2 = this.noWriters;
					this.noWriters = new LinkedList<TaskCompletionSource<bool>>();
				}
			}

			if (!(List is null))
			{
				foreach (TaskCompletionSource<bool> T in List)
					T.TrySetResult(true);
			}

			if (!(List2 is null))
			{
				foreach (TaskCompletionSource<bool> T in List2)
					T.TrySetResult(true);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for writing.
		/// Each successful call to <see cref="TryBeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public virtual async Task<bool> TryBeginWrite(int Timeout)
		{
			TaskCompletionSource<bool> Prev = null;
			TaskCompletionSource<bool> Wait = null;
			DateTime Start = DateTime.Now;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!(Prev is null))
						this.noWriters.Remove(Prev);    // In case previously locked for reading

					if (this.nrReaders == 0 && !this.isWriting)
					{
						this.isWriting = true;
						return true;
					}
					else if (Timeout <= 0)
						return false;
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noWriters.AddLast(Wait);
						this.noReadersOrWriters.AddLast(Wait);
					}
				}

				using (Timer Timer = new Timer((P) =>
				{
					Wait.TrySetResult(false);

				}, null, Timeout, System.Threading.Timeout.Infinite))
				{
					DateTime Now = DateTime.Now;
					bool Result = await Wait.Task;
					if (!Result)
					{
						lock (this.synchObj)
						{
							this.noWriters.Remove(Wait);
							this.noReadersOrWriters.Remove(Wait);
						}

						return false;
					}

					Timeout -= (int)((Now - Start).TotalMilliseconds + 0.5);
					Start = Now;
					Prev = Wait;
				}
			}
		}

		/// <summary>
		/// Unlocks all locks on the object.
		/// </summary>
		public virtual Task Unlock()
		{
			LinkedList<TaskCompletionSource<bool>> List = null;
			LinkedList<TaskCompletionSource<bool>> List2 = null;

			lock (this.synchObj)
			{
				this.nrReaders = 0;
				this.isWriting = false;

				if (!(this.noReadersOrWriters.First is null))
				{
					List = this.noReadersOrWriters;
					this.noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
				}

				if (!(this.noWriters.First is null))
				{
					List2 = this.noWriters;
					this.noWriters = new LinkedList<TaskCompletionSource<bool>>();
				}
			}

			if (!(List is null))
			{
				foreach (TaskCompletionSource<bool> T in List)
					T.TrySetResult(false);
			}

			if (!(List2 is null))
			{
				foreach (TaskCompletionSource<bool> T in List2)
					T.TrySetResult(false);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.Unlock();
		}

	}
}
