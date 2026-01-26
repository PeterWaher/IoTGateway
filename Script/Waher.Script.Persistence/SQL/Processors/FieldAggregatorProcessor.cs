using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Groups;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that adds fields to processed items.
	/// </summary>
	public class FieldAggregatorProcessor : IProcessor<object>
	{
		private readonly KeyValuePair<string, ScriptNode>[] additionalFields;
		private readonly bool[] additionalFieldAsynchronous;
		private readonly IProcessor<object> processor;
		private readonly Variables variables;
		private readonly int count;
		private readonly bool isAsynchronous;
		private ObjectProperties objectVariables = null;

		/// <summary>
		/// Processor that adds fields to processed items.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="AdditionalFields">Fields to add to processed items.</param>
		/// <param name="Columns">Columns to select.</param>
		public FieldAggregatorProcessor(IProcessor<object> Processor, Variables Variables,
			KeyValuePair<string, int>[] AdditionalFields, ScriptNode[] Columns)
		{
			this.processor = Processor;
			this.variables = Variables;

			this.count = AdditionalFields.Length;

			this.additionalFieldAsynchronous = new bool[this.count];
			this.additionalFields = new KeyValuePair<string, ScriptNode>[this.count];
			this.isAsynchronous = Processor.IsAsynchronous;

			for (int i = 0; i < this.count; i++)
			{
				KeyValuePair<string, int> P = AdditionalFields[i];
				ScriptNode Node = Columns[P.Value];

				this.additionalFields[i] = new KeyValuePair<string, ScriptNode>(P.Key, Node);

				if (this.additionalFieldAsynchronous[i] = Node.IsAsynchronous)
					this.isAsynchronous = true;
			}
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
			KeyValuePair<string, ScriptNode> P;
			int i;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (Object is GenericObject GenObj)
			{
				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];
					GenObj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}
			}
			else if (Object is GroupObject GroupObj)
			{
				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];
					GroupObj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}
			}
			else
			{
				GroupObject Obj = new GroupObject(Array.Empty<object>(), Array.Empty<IElement>(), 
					Array.Empty<ScriptNode>(), this.objectVariables);

				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];
					Obj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}

				Object = Obj;
			}

			return this.processor.Process(Object);
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> ProcessAsync(object Object)
		{
			KeyValuePair<string, ScriptNode> P;
			int i;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (Object is GenericObject GenObj)
			{
				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];

					if (this.additionalFieldAsynchronous[i])
						GenObj[P.Key] = await P.Value.EvaluateAsync(this.objectVariables);
					else
						GenObj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}
			}
			else if (Object is GroupObject GroupObj)
			{
				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];

					if (this.additionalFieldAsynchronous[i])
						GroupObj[P.Key] = await P.Value.EvaluateAsync(this.objectVariables);
					else
						GroupObj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}
			}
			else
			{
				GroupObject Obj = new GroupObject(Array.Empty<object>(), Array.Empty<IElement>(), 
					Array.Empty<ScriptNode>(), this.objectVariables);

				for (i = 0; i < this.count; i++)
				{
					P = this.additionalFields[i];

					if (this.additionalFieldAsynchronous[i])
						Obj[P.Key] = await P.Value.EvaluateAsync(this.objectVariables);
					else
						Obj[P.Key] = P.Value.Evaluate(this.objectVariables);
				}

				Object = Obj;
			}

			return await this.processor.ProcessAsync(Object);
		}

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
