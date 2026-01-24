using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that skips a given number of result records.
	/// </summary>
	public class OffsetProcessor : IProcessor<object>
	{
		private readonly IProcessor<object> processor;
		private readonly bool isAsynchronous;
		private int offset;

		/// <summary>
		/// Processor that skips a given number of result records.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		/// <param name="Offset">Number of records to skip.</param>
		public OffsetProcessor(IProcessor<object> Processor, int Offset)
		{
			this.processor = Processor;
			this.offset = Offset;
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
			if (this.offset > 0)
			{
				this.offset--;
				return true;
			}
			else
				return this.processor.Process(Object);
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> ProcessAsync(object Object)
		{
			if (this.offset > 0)
			{
				this.offset--;
				return Task.FromResult(true);
			}
			else
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
