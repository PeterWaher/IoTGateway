using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor of generic objects of type <see cref="GenericObject"/>.
	/// </summary>
	public class TypedObjectProcessor<T> : IProcessor<T>
	{
		private readonly IProcessor<object> processor;

		/// <summary>
		/// Processor of generic objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="Processor">Object processor.</param>
		public TypedObjectProcessor(IProcessor<object> Processor)
		{
			this.processor = Processor;
		}

		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		public bool IsAsynchronous => this.processor.IsAsynchronous;

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Process(T Object) => this.processor.Process(Object);

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> ProcessAsync(T Object) => this.processor.ProcessAsync(Object);

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Flush() => this.processor.Flush();

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> FlushAsync() => this.processor.FlushAsync();
	}
}
