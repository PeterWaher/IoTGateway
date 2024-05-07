using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Task with zero arguments to be synchronized.
    /// </summary>
    /// <typeparam name="ReturnType">Return type of task.</typeparam>
    public class SyncTask0<ReturnType> : ISyncTask
    {
        /// <summary>
        /// Task completion source, waiting for the result of the task.
        /// </summary>
        protected readonly TaskCompletionSource<ReturnType> task;

        private readonly Callback0<ReturnType> callback;

        /// <summary>
        /// Task to be synchronized.
        /// </summary>
        /// <param name="Callback">Method to call when task is executed.</param>
        public SyncTask0(Callback0<ReturnType> Callback)
        {
            this.task = new TaskCompletionSource<ReturnType>();
            this.callback = Callback;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public void Execute()
        {
            try
            {
                this.task.TrySetResult(this.callback());
            }
            catch (Exception ex)
            {
                this.task.TrySetException(ex);
            }
        }

        /// <summary>
        /// Waits for the task to complete.
        /// </summary>
        /// <returns>Task result.</returns>
        public Task<ReturnType> WaitAsync() => this.task.Task;
    }
}
