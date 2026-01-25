using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Processors
{
	/// <summary>
	/// Processor that uses a predicate callback to process objects.
	/// </summary>
	/// <typeparam name="T">Type of object to process</typeparam>
	public class PredicateProcessor<T> : IProcessor<T>
	{
		private readonly Predicate<T> callback;
		
		/// <summary>
		/// Processor that uses a predicate callback to process objects.
		/// </summary>
		/// <param name="Callback">Callback method that returns true if process can
		/// continue and false if it should be cancelled.</param>
		public PredicateProcessor(Predicate<T> Callback)
		{
			this.callback = Callback;
		}

		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		public bool IsAsynchronous => false;

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Process(T Object) => this.callback(Object);

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> ProcessAsync(T Object) => Task.FromResult(this.callback(Object));

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Flush()
		{
			return true;
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> FlushAsync()
		{
			return Task.FromResult(true);
		}
	}
}
