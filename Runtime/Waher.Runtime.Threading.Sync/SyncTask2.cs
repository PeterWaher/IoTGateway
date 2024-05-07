using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Task with one argument to be synchronized.
    /// </summary>
    /// <typeparam name="ReturnType">Return type of task.</typeparam>
    /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
    /// <typeparam name="Arg2Type">Type of second argument.</typeparam>
    public class SyncTask2<ReturnType, Arg1Type, Arg2Type> : ISyncTask
    {
        /// <summary>
        /// Task completion source, waiting for the result of the task.
        /// </summary>
        protected readonly TaskCompletionSource<ReturnType> task;

        private readonly Callback2<ReturnType, Arg1Type, Arg2Type> callback;
        private readonly Arg1Type arg1;
        private readonly Arg2Type arg2;

        /// <summary>
        /// Task to be synchronized.
        /// </summary>
        /// <param name="Callback">Method to call when task is executed.</param>
        /// <param name="Arg1">First argument.</param>
        /// <param name="Arg2">Second argument.</param>
        public SyncTask2(Callback2<ReturnType, Arg1Type, Arg2Type> Callback, Arg1Type Arg1, Arg2Type Arg2)
        {
            this.task = new TaskCompletionSource<ReturnType>();
            this.callback = Callback;
            this.arg1 = Arg1;
            this.arg2 = Arg2;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public void Execute()
        {
            try
            {
                this.task.TrySetResult(this.callback(this.arg1, this.arg2));
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
