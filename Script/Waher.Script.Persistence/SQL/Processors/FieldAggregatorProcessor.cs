using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Groups;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that adds fields to processed items.
	/// </summary>
	public class FieldAggregatorProcessor : IProcessor<object>
	{
		private readonly KeyValuePair<string, int>[] additionalFields;
		private readonly ScriptNode[] columns;
		private readonly IProcessor<object> processor;
		private readonly Variables variables;
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
			this.additionalFields = AdditionalFields;
			this.columns = Columns;
			this.isAsynchronous = Processor.IsAsynchronous;

			if (!(Columns is null))
			{
				foreach (ScriptNode Node in Columns)
				{
					if (Node.IsAsynchronous)
					{
						this.isAsynchronous = true;
						break;
					}
				}
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
			ScriptNode Node;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (Object is GenericObject GenObj)
			{
				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];
					GenObj[P.Key] = Node.Evaluate(this.objectVariables);
				}
			}
			else if (Object is GroupObject GroupObj)
			{
				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];
					GroupObj[P.Key] = Node.Evaluate(this.objectVariables);
				}
			}
			else
			{
				GroupObject Obj = new GroupObject(Array.Empty<object>(), Array.Empty<ScriptNode>(), this.objectVariables);

				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];
					Obj[P.Key] = Node.Evaluate(this.objectVariables);
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
			ScriptNode Node;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (Object is GenericObject GenObj)
			{
				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];

					if (Node.IsAsynchronous)
						GenObj[P.Key] = await Node.EvaluateAsync(this.objectVariables);
					else
						GenObj[P.Key] = Node.Evaluate(this.objectVariables);
				}
			}
			else if (Object is GroupObject GroupObj)
			{
				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];

					if (Node.IsAsynchronous)
						GroupObj[P.Key] = await Node.EvaluateAsync(this.objectVariables);
					else
						GroupObj[P.Key] = Node.Evaluate(this.objectVariables);
				}
			}
			else
			{
				GroupObject Obj = new GroupObject(Array.Empty<object>(), Array.Empty<ScriptNode>(), this.objectVariables);

				foreach (KeyValuePair<string, int> P in this.additionalFields)
				{
					Node = this.columns[P.Value];

					if (Node.IsAsynchronous)
						Obj[P.Key] = await Node.EvaluateAsync(this.objectVariables);
					else
						Obj[P.Key] = Node.Evaluate(this.objectVariables);
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
