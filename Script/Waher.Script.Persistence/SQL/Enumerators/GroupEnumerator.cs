using System;
using System.Collections;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL.Groups;
using Waher.Script.Persistence.SQL.Processors;

namespace Waher.Script.Persistence.SQL.Enumerators
{
	/// <summary>
	/// Enumerator that groups items into groups, and returns aggregated elements.
	/// </summary>
	public class GroupEnumerator : IResultSetEnumerator
	{
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupNames;
		private readonly bool[] groupByAsynchronous;
		private readonly IIterativeEvaluator[] iterators;
		private readonly bool[] iteratorUsesElement;
		private readonly bool[] iteratorAsynchronous;
		private readonly Variables variables;
		private readonly IResultSetEnumerator e;
		private readonly int iteratorCount;
		private bool processLast = false;
		private GroupObject current = null;
		private ObjectProperties objectVariables = null;

		/// <summary>
		/// Enumerator that groups items into groups, and returns aggregated elements.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="GroupBy">Group on these fields</param>
		/// <param name="GroupNames">Names given to grouped fields</param>
		/// <param name="Columns">Columns to select</param>
		/// <param name="Having">Optional having clause</param>
		public GroupEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables,
			ScriptNode[] GroupBy, ScriptNode[] GroupNames, ScriptNode[] Columns,
			ref ScriptNode Having)
		{
			this.e = ItemEnumerator;
			this.variables = Variables;
			this.groupBy = GroupBy;
			this.groupNames = GroupNames;

			int i, c = GroupBy.Length;

			this.groupByAsynchronous = new bool[c];

			for (i = 0; i < c; i++)
				this.groupByAsynchronous[i] = GroupBy[i].IsAsynchronous;

			ChunkedList<IIterativeEvaluator> Iterators = new ChunkedList<IIterativeEvaluator>();

			if (!(Columns is null))
			{
				for (i = 0, c = Columns.Length; i < c; i++)
					GroupProcessor.FindIterators(ref Columns[i], Iterators);
			}

			GroupProcessor.FindIterators(ref Having, Iterators);

			this.iterators = Iterators.ToArray();
			this.iteratorCount = this.iterators.Length;

			this.iteratorUsesElement = new bool[this.iteratorCount];
			this.iteratorAsynchronous = new bool[this.iteratorCount];

			for (i = 0; i < this.iteratorCount; i++)
			{
				this.iteratorUsesElement[i] = this.iterators[i].UsesElement;
				this.iteratorAsynchronous[i] = this.iterators[i].IsAsynchronous;
			}
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			IIterativeEvaluator Iterator;
			IElement E;
			object[] Last = null;
			int i, j, c = this.groupBy.Length;
			object o1, o2;
			bool Found = false;

			while (this.processLast || await this.e.MoveNextAsync())
			{
				this.processLast = false;

				if (this.objectVariables is null)
					this.objectVariables = new ObjectProperties(this.e.Current, this.variables);
				else
					this.objectVariables.Object = this.e.Current;

				if (Last is null)
				{
					Last = new object[c];

					for (i = 0; i < c; i++)
					{
						if (this.groupByAsynchronous[i])
							E = await this.groupBy[i].EvaluateAsync(this.objectVariables);
						else
							E = this.groupBy[i].Evaluate(this.objectVariables);

						Last[i] = E.AssociatedObjectValue;
					}

					if (this.iteratorCount > 0)
					{
						for (j = 0; j < this.iteratorCount; j++)
							this.iterators[j].RestartEvaluator();
					}
				}
				else
				{
					for (i = 0; i < c; i++)
					{
						if (this.groupByAsynchronous[i])
							E = await this.groupBy[i].EvaluateAsync(this.objectVariables);
						else
							E = this.groupBy[i].Evaluate(this.objectVariables);

						o1 = Last[i];
						o2 = E.AssociatedObjectValue;

						if (o1 is null ^ o2 is null)
							break;

						if (!(o1 is null) && !o1.Equals(o2))
							break;
					}

					if (i < c)
					{
						this.processLast = true;
						break;
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

				Found = true;
			}

			if (!Found)
				return false;

			IElement[] Aggregates = new IElement[this.iteratorCount];

			for (i = 0; i < this.iteratorCount; i++)
			{
				Iterator = this.iterators[i];
				Aggregates[i] = Iterator.GetAggregatedResult();
				Iterator.RestartEvaluator();
			}

			this.current = new GroupObject(Last, Aggregates, this.groupNames, this.variables);

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
			this.processLast = false;
			this.current = null;
		}
	}
}
