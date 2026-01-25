using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that generates a tabular record set.
	/// </summary>
	public class RecordProcessor : IProcessor<object>
	{
		private readonly ChunkedList<IElement[]> items = new ChunkedList<IElement[]>();
		private readonly ScriptNode[] columns;
		private readonly bool[] columnAsynchronous;
		private readonly Variables variables;
		private readonly int count;
		private readonly bool isAsynchronous;
		private ObjectProperties properties = null;

		/// <summary>
		/// Processor that generates a tabular record set.
		/// </summary>
		public RecordProcessor(ScriptNode[] Columns, Variables Variables)
		{
			this.columns = Columns;
			this.variables = Variables;
			this.count = this.columns?.Length ?? 0;
			this.columnAsynchronous = new bool[this.count];

			for (int i = 0; i < this.count; i++)
			{
				bool b = this.columnAsynchronous[i] = this.columns[i].IsAsynchronous;
				this.isAsynchronous |= b;
			}
		}

		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		public bool IsAsynchronous => this.isAsynchronous;

		/// <summary>
		/// Number of records processed.
		/// </summary>
		public int Count => this.items.Count;

		/// <summary>
		/// Gets the generated record set.
		/// </summary>
		/// <returns>Array of records.</returns>
		public IElement[][] GetRecordSet()
		{
			return this.items.ToArray();
		}

		/// <summary>
		/// Tries to get a single element result.
		/// </summary>
		/// <param name="Result">Result</param>
		/// <returns>If a single element result could be retrieved.</returns>
		public bool TryGetSingleElement(out IElement Result)
		{
			if (!this.items.HasFirstItem)
			{
				Result = ObjectValue.Null;
				return true;
			}
			else if (this.items.Count == 1 && this.items.FirstItem.Length == 1)
			{
				Result = this.items.FirstItem[0];
				return true;
			}
			else
			{
				Result = null;
				return false;
			}
		}

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Process(object Object)
		{
			IElement Element = Object as IElement;
			IElement[] Record;
			int i;

			if (this.properties is null)
				this.properties = new ObjectProperties(Element?.AssociatedObjectValue ?? Object, this.variables);
			else
				this.properties.Object = Element?.AssociatedObjectValue ?? Object;

			if (this.columns is null)
				Record = new IElement[1] { Element ?? Expression.Encapsulate(Object) };
			else
			{
				Record = new IElement[this.count];

				for (i = 0; i < this.count; i++)
				{
					try
					{
						Record[i] = this.columns[i].Evaluate(this.properties);
					}
					catch (Exception ex)
					{
						ex = Log.UnnestException(ex);
						Record[i] = Expression.Encapsulate(ex);
					}
				}
			}

			this.ProcessRecord(Record);

			return true;
		}

		/// <summary>
		/// Processes a record.
		/// </summary>
		/// <param name="Record">New record.</param>
		protected virtual void ProcessRecord(IElement[] Record)
		{
			this.items.Add(Record);
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> ProcessAsync(object Object)
		{
			IElement Element = Object as IElement;
			IElement[] Record;
			int i;

			if (this.properties is null)
				this.properties = new ObjectProperties(Element?.AssociatedObjectValue ?? Object, this.variables);
			else
				this.properties.Object = Element?.AssociatedObjectValue ?? Object;

			if (this.columns is null)
				Record = new IElement[1] { Element ?? Expression.Encapsulate(Object) };
			else
			{
				Record = new IElement[this.count];

				for (i = 0; i < this.count; i++)
				{
					try
					{
						if (this.columnAsynchronous[i])
							Record[i] = await this.columns[i].EvaluateAsync(this.properties);
						else
							Record[i] = this.columns[i].Evaluate(this.properties);
					}
					catch (Exception ex)
					{
						ex = Log.UnnestException(ex);
						Record[i] = Expression.Encapsulate(ex);
					}
				}
			}

			this.ProcessRecord(Record);

			return true;
		}

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
