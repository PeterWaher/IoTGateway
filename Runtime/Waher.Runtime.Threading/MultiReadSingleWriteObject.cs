using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private static readonly Stopwatch watch = new Stopwatch();

		static MultiReadSingleWriteObject()
		{
			watch.Start();
		}

		private readonly bool recordStackTraces;
		private readonly object owner;
		private readonly string creatorStackTrace;
		private string lockStackTrace;
		private LinkedList<TaskCompletionSource<bool>> noWriters = new LinkedList<TaskCompletionSource<bool>>();
		private LinkedList<TaskCompletionSource<bool>> noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
		private readonly object synchObj = new object();
		private long token = 0;
		private long start = 0;
		private int nrReaders = 0;
		private bool isWriting = false;
		private bool disposed = false;

		/// <summary>
		/// Represents an object that allows single concurrent writers but multiple concurrent readers.
		/// When disposing the object, it automatically ends any reading and writing locks it maintains.
		/// </summary>
		/// <param name="Owner">Owner of object.</param>
		public MultiReadSingleWriteObject(object Owner)
			: this()
		{
			this.owner = Owner;
		}

		/// <summary>
		/// Represents an object that allows single concurrent writers but multiple concurrent readers.
		/// When disposing the object, it automatically ends any reading and writing locks it maintains.
		/// </summary>
		public MultiReadSingleWriteObject()
#if DEBUG
			: this(true)
#else
			: this(false)
#endif
		{
		}

		/// <summary>
		/// Represents an object that allows single concurrent writers but multiple concurrent readers.
		/// When disposing the object, it automatically ends any reading and writing locks it maintains.
		/// </summary>
		public MultiReadSingleWriteObject(object Owner, bool RecordStackTraces)
			: this(RecordStackTraces)
		{
			this.owner = Owner;
		}

		/// <summary>
		/// Represents an object that allows single concurrent writers but multiple concurrent readers.
		/// When disposing the object, it automatically ends any reading and writing locks it maintains.
		/// </summary>
		/// <param name="RecordStackTraces">If stack traces should be recorded when object
		/// is locked. Default value is true in DEBUG mode and false if not in DEBUG mode.</param>
		public MultiReadSingleWriteObject(bool RecordStackTraces)
		{
			this.recordStackTraces = RecordStackTraces;

			if (this.recordStackTraces)
				this.creatorStackTrace = Environment.StackTrace;
			else
				this.creatorStackTrace = null;

			this.lockStackTrace = null;
		}

		/// <summary>
		/// Owner of object.
		/// </summary>
		public object Owner => this.owner;

		/// <summary>
		/// If stack traces should be recorded when object is locked. Default value is true 
		/// in DEBUG mode and false if not in DEBUG mode.
		/// </summary>
		public bool RecordStackTraces => this.recordStackTraces;

		/// <summary>
		/// Stack trace from creation of object, if <see cref="RecordStackTraces"/> is true.
		/// </summary>
		public string CreatorStackTrace => this.creatorStackTrace;

		/// <summary>
		/// Stack trace from lock of object, if <see cref="RecordStackTraces"/> is true.
		/// </summary>
		public string LockStackTrace => this.lockStackTrace;

		/// <summary>
		/// Number of concurrent readers.
		/// </summary>
		public int NrReaders
		{
			get
			{
				lock (this.synchObj)
				{
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

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
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

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
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

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
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

					return this.nrReaders > 0 || this.isWriting;
				}
			}
		}

		/// <summary>
		/// Number of tasks waiting.
		/// </summary>
		public int QueueSize
		{
			get
			{
				lock (this.synchObj)
				{
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));
					
					return this.noReadersOrWriters.Count;
				}
			}
		}

		/// <summary>
		/// Number of ticks the object has been locked.
		/// </summary>
		public long TicksLocked
		{
			get
			{
				lock (this.synchObj)
				{
					if (this.disposed)
						return 0;
					else if (this.nrReaders > 0 || this.isWriting)
						return watch.ElapsedTicks - this.start;
					else
						return 0;
				}
			}
		}

		/// <summary>
		/// Number of milliseconds the object has been locked.
		/// </summary>
		public double MillisecondsLocked => this.TicksLocked * 1000.0 / Stopwatch.Frequency;

		/// <summary>
		/// If object has been disposed.
		/// </summary>
		public bool Disposed => this.disposed;

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the object
		/// is not in a reading state.
		/// </summary>
		public void AssertReading()
		{
			lock (this.synchObj)
			{
				if (this.disposed)
					throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

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
				if (this.disposed)
					throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

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
				if (this.disposed)
					throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

				if (this.nrReaders <= 0 && !this.isWriting)
					throw new InvalidOperationException("Not in a reading or writing state.");
			}
		}

		/// <summary>
		/// Returns a token corresponding to the current lock. It is incremented at the start of a lock-cycle
		/// (when in a state of no locks, to entering the first lock), and when a lock-cyckle ends (when going
		/// from a locked state, to an unlocked state). The token can be used to check, in nested code, if the 
		/// object is in an expected lock, or if a new lock is required.
		/// </summary>
		public long Token
		{
			get
			{
				lock (this.synchObj)
				{
					if (this.disposed)
						throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

					return this.token;
				}
			}
		}

		/// <summary>
		/// Waits until object ready for reading.
		/// Each call to <see cref="BeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <returns>Number of concurrent readers when returning from locked section of call.</returns>
		public virtual async Task<int> BeginRead()
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			TaskCompletionSource<bool> Wait = null;
			int Result = 0;
			bool RecordStackTrace = false;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!this.isWriting)
					{
						if (this.nrReaders == 0)
						{
							this.start = watch.ElapsedTicks;
							this.token++;

							if (this.recordStackTraces)
								RecordStackTrace = true;
						}

						Result = ++this.nrReaders;
					}
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noWriters.AddLast(Wait);
					}
				}

				if (Wait is null)
				{
					if (RecordStackTrace)
						this.lockStackTrace = Environment.StackTrace;

					return Result;
				}
				else
				{
					await Wait.Task;
					Wait = null;
				}
			}
		}

		/// <summary>
		/// Ends a reading session of the object.
		/// Must be called once for each call to <see cref="BeginRead"/> or successful call to <see cref="TryBeginRead(int)"/>.
		/// </summary>
		/// <returns>Number of concurrent readers when returning from locked section of call.</returns>
		public virtual Task<int> EndRead()
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			LinkedList<TaskCompletionSource<bool>> List = null;

			lock (this.synchObj)
			{
				if (this.nrReaders <= 0)
					throw new InvalidOperationException("Not in a reading state.");

				this.nrReaders--;
				if (this.nrReaders == 0)
				{
					this.token++;
					this.start = 0;

					if (this.recordStackTraces)
						this.lockStackTrace = null;
				}
				else
					return Task.FromResult(this.nrReaders);

				if (this.noReadersOrWriters.First is null)
					return Task.FromResult(0);

				List = this.noReadersOrWriters;
				this.noReadersOrWriters = new LinkedList<TaskCompletionSource<bool>>();
			}

			foreach (TaskCompletionSource<bool> T in List)
				T.TrySetResult(true);

			return Task.FromResult(0);
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for reading.
		/// Each successful call to <see cref="TryBeginRead(int)"/> must be followed by exactly 
		/// one call to <see cref="EndRead"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public virtual async Task<bool> TryBeginRead(int Timeout)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			TaskCompletionSource<bool> Wait = null;
			DateTime Start = DateTime.UtcNow;
			bool RecordStackTrace = false;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!this.isWriting)
					{
						if (this.nrReaders == 0)
						{
							this.start = watch.ElapsedTicks;
							this.token++;

							if (this.recordStackTraces)
								RecordStackTrace = true;
						}

						this.nrReaders++;
					}
					else if (Timeout <= 0)
						return false;
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noWriters.AddLast(Wait);
					}
				}

				if (Wait is null)
				{
					if (RecordStackTrace)
						this.lockStackTrace = Environment.StackTrace;

					return true;
				}
				else
				{
					DateTime Now = DateTime.UtcNow;
					bool Result;

					using (Timer Timer = new Timer((P) =>
					{
						Wait?.TrySetResult(false);

					}, null, Timeout, System.Threading.Timeout.Infinite))
					{
						Result = await Wait.Task;
					}

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
					Wait = null;
				}
			}
		}

		/// <summary>
		/// Waits until object ready for reading, or the attempt is cancelled.
		/// Each successful call to <see cref="TryBeginRead(CancellationToken)"/> must be followed 
		/// by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <param name="Cancel">Cancellation token</param>
		/// <returns>If a read lock was obtained (true), or the attempt was cancelled (false).</returns>
		public virtual async Task<bool> TryBeginRead(CancellationToken Cancel)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			if (Cancel.CanBeCanceled)
			{
				TaskCompletionSource<bool> Wait = null;
				bool RecordStackTrace = false;

				Cancel.Register(() =>
				{
					Wait?.TrySetResult(false);
				});

				while (true)
				{
					lock (this.synchObj)
					{
						if (!this.isWriting)
						{
							if (this.nrReaders == 0)
							{
								this.start = watch.ElapsedTicks;
								this.token++;

								if (this.recordStackTraces)
									RecordStackTrace = true;
							}

							this.nrReaders++;
						}
						else if (Cancel.IsCancellationRequested)
							return false;
						else
						{
							Wait = new TaskCompletionSource<bool>();
							this.noWriters.AddLast(Wait);
						}
					}

					if (Wait is null)
					{
						if (RecordStackTrace)
							this.lockStackTrace = Environment.StackTrace;

						return true;
					}
					else
					{
						bool Result = await Wait.Task;

						if (!Result)
						{
							lock (this.synchObj)
							{
								this.noWriters.Remove(Wait);
							}

							return false;
						}

						Wait = null;
					}
				}
			}
			else
			{
				await this.BeginRead();
				return true;
			}
		}

		/// <summary>
		/// Waits until object ready for writing.
		/// Each call to <see cref="BeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		public virtual async Task BeginWrite()
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			TaskCompletionSource<bool> Prev = null;
			TaskCompletionSource<bool> Wait = null;
			bool RecordStackTrace = false;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!(Prev is null))
					{
						this.noWriters.Remove(Prev);    // In case previously locked for reading
						this.noReadersOrWriters.Remove(Prev);
					}

					if (this.nrReaders == 0 && !this.isWriting)
					{
						this.start = watch.ElapsedTicks;
						this.token++;
						this.isWriting = true;

						if (this.recordStackTraces)
							RecordStackTrace = true;
					}
					else
					{
						Wait = new TaskCompletionSource<bool>();
						this.noReadersOrWriters.AddLast(Wait);
						this.noWriters.AddLast(Wait);
					}
				}

				if (Wait is null)
				{
					if (RecordStackTrace)
						this.lockStackTrace = Environment.StackTrace;

					return;
				}
				else
				{
					await Wait.Task;
					Prev = Wait;
					Wait = null;
				}
			}
		}

		/// <summary>
		/// Ends a writing session of the object.
		/// Must be called once for each call to <see cref="BeginWrite"/> or successful call to <see cref="TryBeginWrite(int)"/>.
		/// </summary>
		public virtual Task EndWrite()
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			LinkedList<TaskCompletionSource<bool>> List = null;
			LinkedList<TaskCompletionSource<bool>> List2 = null;

			lock (this.synchObj)
			{
				if (!this.isWriting)
					throw new InvalidOperationException("Not in a writing state.");

				this.token++;
				this.isWriting = false;
				this.start = 0;

				if (this.recordStackTraces)
					this.lockStackTrace = null;

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
		/// Each successful call to <see cref="TryBeginWrite(int)"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>If a write lock was obtained within the given time.</returns>
		public virtual async Task<bool> TryBeginWrite(int Timeout)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			TaskCompletionSource<bool> Prev = null;
			TaskCompletionSource<bool> Wait = null;
			DateTime Start = DateTime.UtcNow;
			bool RecordStackTrace = false;

			while (true)
			{
				lock (this.synchObj)
				{
					if (!(Prev is null))
					{
						this.noWriters.Remove(Prev);    // In case previously locked for reading
						this.noReadersOrWriters.Remove(Prev);
					}

					if (this.nrReaders == 0 && !this.isWriting)
					{
						this.start = watch.ElapsedTicks;
						this.token++;
						this.isWriting = true;

						if (this.recordStackTraces)
							RecordStackTrace = true;
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

				if (Wait is null)
				{
					if (RecordStackTrace)
						this.lockStackTrace = Environment.StackTrace;

					return true;
				}
				else
				{
					DateTime Now = DateTime.UtcNow;
					bool Result;

					using (Timer Timer = new Timer((P) =>
					{
						Wait?.TrySetResult(false);

					}, null, Timeout, System.Threading.Timeout.Infinite))
					{
						Result = await Wait.Task;
					}

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
					Wait = null;
				}
			}
		}

		/// <summary>
		/// Waits until object ready for writing, or the attempt is cancelled.
		/// Each successful call to <see cref="TryBeginWrite(CancellationToken)"/> must be followed 
		/// by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <param name="Cancel">Cancellation token</param>
		/// <returns>If a write lock was obtained (true), or the attempt was cancelled (false).</returns>
		public virtual async Task<bool> TryBeginWrite(CancellationToken Cancel)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));

			if (Cancel.CanBeCanceled)
			{
				TaskCompletionSource<bool> Prev = null;
				TaskCompletionSource<bool> Wait = null;
				DateTime Start = DateTime.UtcNow;
				bool RecordStackTrace = false;

				Cancel.Register(() =>
				{
					Prev?.TrySetResult(false);
					Wait?.TrySetResult(false);
				});

				while (true)
				{
					lock (this.synchObj)
					{
						if (!(Prev is null))
						{
							this.noWriters.Remove(Prev);    // In case previously locked for reading
							this.noReadersOrWriters.Remove(Prev);
						}

						if (this.nrReaders == 0 && !this.isWriting)
						{
							this.start = watch.ElapsedTicks;
							this.token++;
							this.isWriting = true;

							if (this.recordStackTraces)
								RecordStackTrace = true;
						}
						else if (Cancel.IsCancellationRequested)
							return false;
						else
						{
							Wait = new TaskCompletionSource<bool>();
							this.noWriters.AddLast(Wait);
							this.noReadersOrWriters.AddLast(Wait);
						}
					}

					if (Wait is null)
					{
						if (RecordStackTrace)
							this.lockStackTrace = Environment.StackTrace;

						return true;
					}
					else
					{
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

						Prev = Wait;
						Wait = null;
					}
				}
			}
			else
			{
				await this.BeginWrite();
				return true;
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
				this.token++;
				this.start = 0;

				if (this.recordStackTraces)
					this.lockStackTrace = null;

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
			if (this.disposed)
				throw new ObjectDisposedException(nameof(MultiReadSingleWriteObject));
			else
			{
				this.disposed = true;
				this.Unlock();
			}
		}

	}
}
