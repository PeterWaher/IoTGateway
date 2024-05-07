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
    /// <typeparam name="Arg3Type">Type of third argument.</typeparam>
    public class SyncTask3<ReturnType, Arg1Type, Arg2Type, Arg3Type> : ISyncTask
    {
        /// <summary>
        /// Task completion source, waiting for the result of the task.
        /// </summary>
        protected readonly TaskCompletionSource<ReturnType> task;

        private readonly Callback3<ReturnType, Arg1Type, Arg2Type, Arg3Type> callback;
        private readonly Arg1Type arg1;
        private readonly Arg2Type arg2;
        private readonly Arg3Type arg3;

        /// <summary>
        /// Task to be synchronized.
        /// </summary>
        /// <param name="Callback">Method to call when task is executed.</param>
        /// <param name="Arg1">First argument.</param>
        /// <param name="Arg2">Second argument.</param>
        /// <param name="Arg3">Third argument.</param>
        public SyncTask3(Callback3<ReturnType, Arg1Type, Arg2Type, Arg3Type> Callback, Arg1Type Arg1, Arg2Type Arg2, 
            Arg3Type Arg3)
        {
            this.task = new TaskCompletionSource<ReturnType>();
            this.callback = Callback;
            this.arg1 = Arg1;
            this.arg2 = Arg2;
            this.arg3 = Arg3;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public void Execute()
        {
            try
            {
                this.task.TrySetResult(this.callback(this.arg1, this.arg2, this.arg3));
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
