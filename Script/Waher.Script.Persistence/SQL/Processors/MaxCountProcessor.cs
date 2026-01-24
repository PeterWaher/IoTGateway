using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that limits the return set to a maximum number of records.
	/// </summary>
	public class MaxCountProcessor : IProcessor<object>
	{
		private readonly IProcessor<object> processor;
		private readonly bool isAsynchronous;
		private int count;

		/// <summary>
		/// Enumerator that limits the return set to a maximum number of records.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		/// <param name="Count">Maximum number of records to enumerate.</param>
		public MaxCountProcessor(IProcessor<object> Processor, int Count)
		{
			this.processor = Processor;
			this.count = Count;
			this.isAsynchronous = Processor.IsAsynchronous;
		}

		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		public bool IsAsynchronous => this.isAsynchronous;

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Process(object Object)
		{
			if (this.count <= 0)
				return false;

			this.count--;
			return this.processor.Process(Object);
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> ProcessAsync(object Object)
		{
			if (this.count <= 0)
				return Task.FromResult(false);

			this.count--;
			return this.processor.ProcessAsync(Object);
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		public void Flush() => this.processor.Flush();

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		public Task FlushAsync() => this.processor.FlushAsync();
	}
}
