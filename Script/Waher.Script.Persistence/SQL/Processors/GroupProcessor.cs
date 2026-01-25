using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL.Groups;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that groups items into groups, and processes aggregated elements.
	/// </summary>
	public class GroupProcessor : IProcessor<object>
	{
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupNames;
		private readonly bool[] groupByAsynchronous;
		private readonly IIterativeEvaluator[] iterators;
		private readonly bool[] iteratorUsesElement;
		private readonly bool[] iteratorAsynchronous;
		private readonly Variables variables;
		private readonly int iteratorCount;
		private readonly int count;
		private readonly bool isAsynchronous;
		private IProcessor<object> processor;
		private ObjectProperties objectVariables = null;
		private object[] last = null;
		private object[] current = null;

		/// <summary>
		/// Processor that groups items into groups, and processes aggregated elements.
		/// </summary>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="GroupBy">Group on these fields</param>
		/// <param name="GroupNames">Names given to grouped fields</param>
		/// <param name="Columns">Columns to select</param>
		/// <param name="Having">Optional having clause</param>
		public GroupProcessor(Variables Variables,
			ScriptNode[] GroupBy, ScriptNode[] GroupNames, ScriptNode[] Columns,
			ref ScriptNode Having)
		{
			this.variables = Variables;
			this.groupBy = GroupBy;
			this.groupNames = GroupNames;
			this.isAsynchronous = false;

			int i, c = GroupBy.Length;
			bool b;

			this.groupByAsynchronous = new bool[c];
			this.count = c;

			for (i = 0; i < c; i++)
			{
				b = this.groupByAsynchronous[i] = GroupBy[i].IsAsynchronous;
				this.isAsynchronous |= b;
			}

			if (!this.isAsynchronous && !(GroupNames is null))
			{
				for (i = 0, c = GroupNames.Length; i < c; i++)
				{
					if (GroupNames[i].IsAsynchronous)
					{
						this.isAsynchronous = true;
						break;
					}
				}
			}

			if (!this.isAsynchronous && !(Columns is null))
			{
				for (i = 0, c = Columns.Length; i < c; i++)
				{
					if (Columns[i].IsAsynchronous)
					{
						this.isAsynchronous = true;
						break;
					}
				}
			}

			if (!this.isAsynchronous && !(Having is null))
				this.isAsynchronous |= Having.IsAsynchronous;

			ChunkedList<IIterativeEvaluator> Iterators = new ChunkedList<IIterativeEvaluator>();

			if (!(Columns is null))
			{
				for (i = 0, c = Columns.Length; i < c; i++)
					FindIterators(ref Columns[i], Iterators);
			}

			FindIterators(ref Having, Iterators);

			this.iterators = Iterators.ToArray();
			this.iteratorCount = this.iterators.Length;

			this.iteratorUsesElement = new bool[this.iteratorCount];
			this.iteratorAsynchronous = new bool[this.iteratorCount];

			for (i = 0; i < this.iteratorCount; i++)
			{
				this.iteratorUsesElement[i] = this.iterators[i].UsesElement;
				this.iteratorAsynchronous[i] = b = this.iterators[i].IsAsynchronous;
				this.isAsynchronous |= b;
			}
		}

		/// <summary>
		/// Sets the inner processor.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		public void SetInnerProcessor(IProcessor<object> Processor)
		{
			this.processor = Processor;
		}

		internal static void FindIterators(ref ScriptNode Node, ChunkedList<IIterativeEvaluator> Iterators)
		{
			if (Node is IIterativeEvaluation IterativeEvaluation2)
			{
				IIterativeEvaluator Evaluator = IterativeEvaluation2.CreateEvaluator();
				Iterators.Add(Evaluator);

				ScriptNode Node0 = Node;

				Node = new GroupIteratorValue(Evaluator, Node.Start, Node.Length,
					Node.Expression);
				Node.SetParent(Node0.Parent);
			}
			else if (Node is GroupIteratorValue GroupIteratorValue2)
				Iterators.Add(GroupIteratorValue2.Iterator);
			else
				Node?.ForAllChildNodes(FindIterators, Iterators, SearchMethod.TreeOrder);
		}

		internal static bool FindIterators(ScriptNode Node, out ScriptNode NewNode, object State)
		{
			NewNode = null;

			if (Node is IIterativeEvaluation IterativeEvaluation)
			{
				ChunkedList<IIterativeEvaluator> Iterators = (ChunkedList<IIterativeEvaluator>)State;
				IIterativeEvaluator Evaluator = IterativeEvaluation.CreateEvaluator();
				Iterators.Add(Evaluator);

				NewNode = new GroupIteratorValue(Evaluator, Node.Start, Node.Length,
					Node.Expression);
			}
			else if (Node is GroupIteratorValue GroupIteratorValue)
			{
				ChunkedList<IIterativeEvaluator> Iterators = (ChunkedList<IIterativeEvaluator>)State;
				Iterators.Add(GroupIteratorValue.Iterator);
			}

			return true;
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
			IIterativeEvaluator Iterator;
			IElement E;
			object o1, o2;
			int i, j;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (this.last is null)
			{
				this.last = new object[this.count];

				for (i = 0; i < this.count; i++)
				{
					E = this.groupBy[i].Evaluate(this.objectVariables);
					this.last[i] = E.AssociatedObjectValue;
				}

				if (this.iteratorCount > 0)
				{
					for (j = 0; j < this.iteratorCount; j++)
						this.iterators[j].RestartEvaluator();
				}
			}
			else
			{
				if (this.current is null)
					this.current = new object[this.count];

				bool Same = true;

				for (i = 0; i < this.count; i++)
				{
					E = this.groupBy[i].Evaluate(this.objectVariables);

					o1 = this.last[i];
					o2 = this.current[i] = E.AssociatedObjectValue;

					if ((o1 is null ^ o2 is null) ||
						(!(o1 is null) && !o1.Equals(o2)))
					{
						Same = false;
					}
				}

				if (!Same)
				{
					if (this.processor is null)
						return false;

					GroupObject Obj = new GroupObject(this.last, this.groupNames, this.variables);
					if (!this.processor.Process(Obj))
						return false;

					this.last = this.current;
					this.current = null;

					if (this.iteratorCount > 0)
					{
						for (j = 0; j < this.iteratorCount; j++)
							this.iterators[j].RestartEvaluator();
					}
				}
			}

			if (this.iteratorCount > 0)
			{
				for (j = 0; j < this.iteratorCount; j++)
				{
					Iterator = this.iterators[j];

					if (this.iteratorUsesElement[j])
					{
						E = Iterator.Evaluate(this.objectVariables);
						Iterator.AggregateElement(E);
					}
					else
						Iterator.AggregateElement(ObjectValue.Null);
				}
			}

			return true;
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> ProcessAsync(object Object)
		{
			IIterativeEvaluator Iterator;
			IElement E;
			object o1, o2;
			int i, j;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(Object, this.variables);
			else
				this.objectVariables.Object = Object;

			if (this.last is null)
			{
				this.last = new object[this.count];

				for (i = 0; i < this.count; i++)
				{
					if (this.groupByAsynchronous[i])
						E = await this.groupBy[i].EvaluateAsync(this.objectVariables);
					else
						E = this.groupBy[i].Evaluate(this.objectVariables);

					this.last[i] = E.AssociatedObjectValue;
				}

				if (this.iteratorCount > 0)
				{
					for (j = 0; j < this.iteratorCount; j++)
						this.iterators[j].RestartEvaluator();
				}
			}
			else
			{
				if (this.current is null)
					this.current = new object[this.count];

				bool Same = true;

				for (i = 0; i < this.count; i++)
				{
					if (this.groupByAsynchronous[i])
						E = await this.groupBy[i].EvaluateAsync(this.objectVariables);
					else
						E = this.groupBy[i].Evaluate(this.objectVariables);

					o1 = this.last[i];
					o2 = this.current[i] = E.AssociatedObjectValue;

					if ((o1 is null ^ o2 is null) ||
						(!(o1 is null) && !o1.Equals(o2)))
					{
						Same = false;
					}
				}

				if (!Same)
				{
					if (this.processor is null)
						return false;

					GroupObject Obj = new GroupObject(this.last, this.groupNames, this.variables);
					if (!await this.processor.ProcessAsync(Obj))
						return false;

					this.last = this.current;
					this.current = null;

					if (this.iteratorCount > 0)
					{
						for (j = 0; j < this.iteratorCount; j++)
							this.iterators[j].RestartEvaluator();
					}
				}
			}

			if (this.iteratorCount > 0)
			{
				for (j = 0; j < this.iteratorCount; j++)
				{
					Iterator = this.iterators[j];

					if (this.iteratorUsesElement[j])
					{
						if (this.iteratorAsynchronous[j])
							E = await Iterator.EvaluateAsync(this.objectVariables);
						else
							E = Iterator.Evaluate(this.objectVariables);

						Iterator.AggregateElement(E);
					}
					else
						Iterator.AggregateElement(ObjectValue.Null);
				}
			}

			return true;
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Flush()
		{
			if (!(this.last is null))
			{
				GroupObject Obj = new GroupObject(this.last, this.groupNames, this.variables);
				if (!this.processor.Process(Obj))
					return false;
			}

			return this.processor.Flush();
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> FlushAsync()
		{
			if (!(this.last is null))
			{
				GroupObject Obj = new GroupObject(this.last, this.groupNames, this.variables);
				if (!await this.processor.ProcessAsync(Obj))
					return false;
			}

			return await this.processor.FlushAsync();
		}
	}
}
