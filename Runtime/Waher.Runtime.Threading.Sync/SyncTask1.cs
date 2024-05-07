using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Task with one argument to be synchronized.
    /// </summary>
    /// <typeparam name="ReturnType">Return type of task.</typeparam>
    /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
    public class SyncTask1<ReturnType, Arg1Type> : ISyncTask
    {
        /// <summary>
        /// Task completion source, waiting for the result of the task.
        /// </summary>
        protected readonly TaskCompletionSource<ReturnType> task;

        private readonly Callback1<ReturnType, Arg1Type> callback;
        private readonly Arg1Type arg1;

        /// <summary>
        /// Task to be synchronized.
        /// </summary>
        /// <param name="Callback">Method to call when task is executed.</param>
        /// <param name="Arg1">First argument.</param>
        public SyncTask1(Callback1<ReturnType, Arg1Type> Callback, Arg1Type Arg1)
        {
            this.task = new TaskCompletionSource<ReturnType>();
            this.callback = Callback;
            this.arg1 = Arg1;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public void Execute()
        {
            try
            {
                this.task.TrySetResult(this.callback(this.arg1));
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
