using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Delegate to methods of zero parameters and a given return type.
    /// </summary>
    /// <typeparam name="ReturnType">Return type.</typeparam>
    /// <returns>Result</returns>
    public delegate ReturnType Callback0<ReturnType>();

    /// <summary>
    /// Delegate to methods of one parameter and a given return type.
    /// </summary>
    /// <typeparam name="ReturnType">Return type.</typeparam>
    /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
    /// <param name="Arg1">First argument.</param>
    /// <returns>Result</returns>
    public delegate ReturnType Callback1<ReturnType, Arg1Type>(Arg1Type Arg1);

    /// <summary>
    /// Delegate to methods of two parameters and a given return type.
    /// </summary>
    /// <typeparam name="ReturnType">Return type.</typeparam>
    /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
    /// <typeparam name="Arg2Type">Type of second argument.</typeparam>
    /// <param name="Arg1">First argument.</param>
    /// <param name="Arg2">Second argument.</param>
    /// <returns>Result</returns>
    public delegate ReturnType Callback2<ReturnType, Arg1Type, Arg2Type>(Arg1Type Arg1, Arg2Type Arg2);

    /// <summary>
    /// Delegate to methods of three parameters and a given return type.
    /// </summary>
    /// <typeparam name="ReturnType">Return type.</typeparam>
    /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
    /// <typeparam name="Arg2Type">Type of second argument.</typeparam>
    /// <typeparam name="Arg3Type">Type of third argument.</typeparam>
    /// <param name="Arg1">First argument.</param>
    /// <param name="Arg2">Second argument.</param>
    /// <param name="Arg3">Third argument.</param>
    /// <returns>Result</returns>
    public delegate ReturnType Callback3<ReturnType, Arg1Type, Arg2Type, Arg3Type>(Arg1Type Arg1, Arg2Type Arg2, Arg3Type Arg3);

    /// <summary>
    /// Class that executes tasks from the same the same thread, and that provides an asynchronous interface
    /// for waiting for responses.
    /// </summary>
    public class ThreadExecutor : IDisposable
    {
        private readonly ManualResetEvent terminate = new ManualResetEvent(false);
        private readonly AutoResetEvent newTask = new AutoResetEvent(false);
        private readonly LinkedList<ISyncTask> tasks = new LinkedList<ISyncTask>();
        private Thread thread;
        private bool terminating = false;
        private bool terminated = false;
        private bool idle = true;
        private bool working = false;
        
        /// <summary>
        /// Class that executes tasks from the same the same thread, and that provides an asynchronous interface
        /// for waiting for responses.
        /// </summary>
        public ThreadExecutor()
        {
            this.thread = new Thread(this.Executor);
            this.thread.Start();
        }

        /// <summary>
        /// Disposes of the object and terminates the thread executor.
        /// </summary>
        public void Dispose()
        {
            if (!this.terminating && !this.terminated)
            {
                this.terminating = true;
                this.terminate?.Set();
            }
        }

        /// <summary>
        /// If execution thread is terminating.
        /// </summary>
        public bool Terminating => this.terminating;

        /// <summary>
        /// If execution thread has terminated.
        /// </summary>
        public bool Terminated => this.terminated;

        /// <summary>
        /// If thread is idle
        /// </summary>
        public bool Idle => this.idle;

        /// <summary>
        /// If thread is working.
        /// </summary>
        public bool Working => this.working;

        private void Add(ISyncTask Task)
        {
            if (this.terminating || this.terminated)
                throw new InvalidOperationException("Thread executer is being terminated or has been terminated.");
                
            lock (this.tasks)
            {
                this.tasks.AddLast(Task);
                this.newTask.Set();
            }
        }

        /// <summary>
        /// Executes a task with no arguments.
        /// </summary>
        /// <typeparam name="ReturnType">Type of result.</typeparam>
        /// <param name="Callback">Callback method called during execution of task.</param>
        /// <returns>Response from callback method.</returns>
        public Task<ReturnType> Execute<ReturnType>(Callback0<ReturnType> Callback)
        {
            SyncTask0<ReturnType> Result = new SyncTask0<ReturnType>(Callback);
            this.Add(Result);
            return Result.WaitAsync();
        }

        /// <summary>
        /// Executes a task with one argument.
        /// </summary>
        /// <typeparam name="ReturnType">Type of result.</typeparam>
        /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
        /// <param name="Callback">Callback method called during execution of task.</param>
        /// <param name="Arg1">First argument.</param>
        /// <returns>Response from callback method.</returns>
        public Task<ReturnType> Execute<ReturnType, Arg1Type>(Callback1<ReturnType, Arg1Type> Callback,
            Arg1Type Arg1)
        {
            SyncTask1<ReturnType, Arg1Type> Result = new SyncTask1<ReturnType, Arg1Type>(Callback, Arg1);
            this.Add(Result);
            return Result.WaitAsync();
        }

        /// <summary>
        /// Executes a task with one argument.
        /// </summary>
        /// <typeparam name="ReturnType">Type of result.</typeparam>
        /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
        /// <typeparam name="Arg2Type">Type of second argument.</typeparam>
        /// <param name="Callback">Callback method called during execution of task.</param>
        /// <param name="Arg1">First argument.</param>
        /// <param name="Arg2">Second argument.</param>
        /// <returns>Response from callback method.</returns>
        public Task<ReturnType> Execute<ReturnType, Arg1Type, Arg2Type>(
            Callback2<ReturnType, Arg1Type, Arg2Type> Callback, Arg1Type Arg1, Arg2Type Arg2)
        {
            SyncTask2<ReturnType, Arg1Type, Arg2Type> Result = new SyncTask2<ReturnType, Arg1Type, Arg2Type>(
                Callback, Arg1, Arg2);
            this.Add(Result);
            return Result.WaitAsync();
        }

        /// <summary>
        /// Executes a task with one argument.
        /// </summary>
        /// <typeparam name="ReturnType">Type of result.</typeparam>
        /// <typeparam name="Arg1Type">Type of first argument.</typeparam>
        /// <typeparam name="Arg2Type">Type of second argument.</typeparam>
        /// <typeparam name="Arg3Type">Type of third argument.</typeparam>
        /// <param name="Callback">Callback method called during execution of task.</param>
        /// <param name="Arg1">First argument.</param>
        /// <param name="Arg2">Second argument.</param>
        /// <param name="Arg3">Third argument.</param>
        /// <returns>Response from callback method.</returns>
        public Task<ReturnType> Execute<ReturnType, Arg1Type, Arg2Type, Arg3Type>(
            Callback3<ReturnType, Arg1Type, Arg2Type, Arg3Type> Callback, Arg1Type Arg1, Arg2Type Arg2,
            Arg3Type Arg3)
        {
            SyncTask3<ReturnType, Arg1Type, Arg2Type, Arg3Type> Result = 
                new SyncTask3<ReturnType, Arg1Type, Arg2Type, Arg3Type>(Callback, Arg1, Arg2, Arg3);
            this.Add(Result);
            return Result.WaitAsync();
        }

        private void Executor()
        {
            try
            {
                WaitHandle[] Handles = new WaitHandle[] { this.newTask, this.terminate };
                ISyncTask ToExecute;
                bool Continue = true;

                while (Continue)
                {
                    switch (WaitHandle.WaitAny(Handles, 1000))
                    {
                        case 0:
                            do
                            {
                                lock (this.tasks)
                                {
                                    if (this.tasks.First is null)
                                        ToExecute = null;
                                    else
                                    {
                                        ToExecute = this.tasks.First.Value;
                                        this.tasks.RemoveFirst();
                                    }
                                }

                                if (!(ToExecute is null))
                                {
                                    try
                                    {
                                        ToExecute.Execute();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Exception(ex);
                                    }
                                }
                            }
                            while (!(ToExecute is null));
                            break;

                        case 1:
                            Continue = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {
                this.terminating = false;
                this.terminated = true;
                this.idle = false;
                this.working = false;
                this.thread = null;

                this.terminate.Dispose();
                this.newTask.Dispose();
            }
        }
    }
}