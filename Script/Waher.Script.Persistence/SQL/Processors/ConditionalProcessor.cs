using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that only processes elements matching a set of conditions.
	/// </summary>
	public class ConditionalProcessor : IProcessor<object>
	{
		private readonly IProcessor<object> processor;
		private readonly bool isAsynchronous;
		private readonly ScriptNode conditions;
		private readonly Variables variables;
		private ObjectProperties properties = null;

		/// <summary>
		/// Processor that only processes elements matching a set of conditions.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Conditions">Set of conditions that must be fulfilled.</param>
		public ConditionalProcessor(IProcessor<object> Processor, Variables Variables, ScriptNode Conditions)
		{
			this.processor = Processor;
			this.variables = Variables;
			this.conditions = Conditions;
			this.isAsynchronous = Processor.IsAsynchronous || Conditions.IsAsynchronous;
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
			try
			{
				if (this.properties is null)
					this.properties = new ObjectProperties(Object, this.variables);
				else
					this.properties.Object = Object;

				IElement E = this.conditions.Evaluate(this.properties);
				if (!(E.AssociatedObjectValue is bool B) || !B)
					return true;

				return this.processor.Process(Object);
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> ProcessAsync(object Object)
		{
			try
			{
				if (this.properties is null)
					this.properties = new ObjectProperties(Object, this.variables);
				else
					this.properties.Object = Object;

				IElement E = await this.conditions.EvaluateAsync(this.properties);
				if (!(E.AssociatedObjectValue is bool B) || !B)
					return true;

				return await this.processor.ProcessAsync(Object);
			}
			catch (Exception)
			{
				return true;
			}
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
