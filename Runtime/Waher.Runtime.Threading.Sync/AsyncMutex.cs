using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Asynchronous mutex class.
    /// </summary>
    public class AsyncMutex : IDisposable
    {
        private readonly object synchObj = new object();
        private ThreadExecutor executor = null;
        private Mutex mutex;

        /// <summary>
        /// Asynchronous mutex class.
        /// </summary>
        public AsyncMutex()
            : this(new Mutex())
        {
        }

        /// <summary>
        /// Asynchronous mutex class.
        /// </summary>
        /// <param name="InitiallyOwned">if mutex is initially owned by caller.</param>
        public AsyncMutex(bool InitiallyOwned)
            : this(new Mutex(InitiallyOwned))
        {
        }

        /// <summary>
        /// Asynchronous mutex class.
        /// </summary>
        /// <param name="InitiallyOwned">if mutex is initially owned by caller.</param>
        /// <param name="Name">Name of mutex.</param>
        public AsyncMutex(bool InitiallyOwned, string Name)
            : this(new Mutex(InitiallyOwned, Name))
        {
        }

        /// <summary>
        /// Asynchronous mutex class.
        /// </summary>
        /// <param name="InitiallyOwned">if mutex is initially owned by caller.</param>
        /// <param name="Name">Name of mutex.</param>
        /// <param name="CreatedNew">If a new mutex was created.</param>
        public AsyncMutex(bool InitiallyOwned, string Name, out bool CreatedNew)
            : this(new Mutex(InitiallyOwned, Name, out CreatedNew))
        {
        }

        /// <summary>
        /// Asynchronous mutex class.
        /// </summary>
        /// <param name="Mutex">Mutex object</param>
        public AsyncMutex(Mutex Mutex)
            : base()
        {
            this.mutex = Mutex;
        }

        /// <summary>
        /// Opens an existing Mutex.
        /// </summary>
        /// <param name="Name">Name of mutex.</param>
        /// <returns>Asynchronous Mutex.</returns>
        public static AsyncMutex OpenExisting(string Name)
        {
            return new AsyncMutex(Mutex.OpenExisting(Name));
        }

        /// <summary>
        /// Tries to open an existing Mutex.
        /// </summary>
        /// <param name="Name">Name of Mutex.</param>
        /// <param name="Result">Asychronous Mutex, if found.</param>
        /// <returns>If able to open Mutex.</returns>
        public static bool TryOpenExisting(string Name, out AsyncMutex Result)
        {
            if (!Mutex.TryOpenExisting(Name, out Mutex MutexObj))
            {
                Result = null;
                return false;
            }

            Result = new AsyncMutex(MutexObj);

            return true;
        }

        private ThreadExecutor Executor
        {
            get
            {
                lock (this.synchObj)
                {
                    if (this.executor is null)
                        this.executor = new ThreadExecutor();

                    return this.executor;
                }
            }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            this.executor?.Dispose();
            this.executor = null;

            this.mutex?.Dispose();
            this.mutex = null;
        }

        /// <summary>
        /// Waits for the Mutex to be free, and locks it.
        /// </summary>
        /// <returns>If successful.</returns>
        public Task<bool> WaitOne()
        {
            return this.Executor.Execute(this.WaitOneSync);
        }

        private bool WaitOneSync()
        {
            return this.mutex.WaitOne();
        }

        /// <summary>
        /// Waits for the Mutex to be free, and locks it.
        /// </summary>
        /// <param name="MillisecondsTimeout">Timeout, in milliseconds.</param>
        /// <returns>If successful.</returns>
        public Task<bool> WaitOne(int MillisecondsTimeout)
        {
            return this.Executor.Execute(this.WaitOneSync, MillisecondsTimeout);
        }

        private bool WaitOneSync(int MillisecondsTimeout)
        {
            return this.mutex.WaitOne(MillisecondsTimeout);
        }

        /// <summary>
        /// Waits for the Mutex to be free, and locks it.
        /// </summary>
        /// <param name="MillisecondsTimeout">Timeout, in milliseconds.</param>
        /// <param name="ExitContext">true to exit the synchronization domain for the context before the wait 
        /// (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
        /// <returns>If successful.</returns>
        public Task<bool> WaitOne(int MillisecondsTimeout, bool ExitContext)
        {
            return this.Executor.Execute(this.WaitOneSync, MillisecondsTimeout, ExitContext);
        }

        private bool WaitOneSync(int MillisecondsTimeout, bool ExitContext)
        {
            return this.mutex.WaitOne(MillisecondsTimeout, ExitContext);
        }

        /// <summary>
        /// Waits for the Mutex to be free, and locks it.
        /// </summary>
        /// <param name="Timeout">Timeout.</param>
        /// <returns>If successful.</returns>
        public Task<bool> WaitOne(TimeSpan Timeout)
        {
            return this.Executor.Execute<bool, TimeSpan>(this.WaitOneSync, Timeout);
        }

        private bool WaitOneSync(TimeSpan Timeout)
        {
            return this.mutex.WaitOne(Timeout);
        }

        /// <summary>
        /// Waits for the Mutex to be free, and locks it.
        /// </summary>
        /// <param name="Timeout">Timeout.</param>
        /// <param name="ExitContext">true to exit the synchronization domain for the context before the wait 
        /// (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
        /// <returns>If successful.</returns>
        public Task<bool> WaitOne(TimeSpan Timeout, bool ExitContext)
        {
            return this.Executor.Execute(this.WaitOneSync, Timeout, ExitContext);
        }

        private bool WaitOneSync(TimeSpan Timeout, bool ExitContext)
        {
            return this.mutex.WaitOne(Timeout, ExitContext);
        }

        /// <summary>
        /// Releases the mutex earlier aquired via a call to WaitOne.
        /// </summary>
        public async Task ReleaseMutex()
        {
            await this.Executor.Execute(this.ReleaseMutexSync);
        }

        private bool ReleaseMutexSync()
        {
            this.mutex.ReleaseMutex();
            return true;
        }
    }
}
