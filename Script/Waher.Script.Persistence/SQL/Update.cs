using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Assignments;
using Waher.Script.Persistence.SQL.Enumerators;

namespace Waher.Script.Persistence.SQL
{
    /// <summary>
    /// Executes an UPDATE statement against the object database.
    /// </summary>
    public class Update : ScriptNode, IEvaluateAsync
	{
		private readonly Assignment[] setOperations;
		private SourceDefinition source;
		private ScriptNode where;
		private ObjectProperties properties = null;
		private readonly bool lazy;

		/// <summary>
		/// Executes an UPDATE statement against the object database.
		/// </summary>
		/// <param name="Source">Source to update objects from.</param>
		/// <param name="SetOperations">Set operations to perform.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Update(SourceDefinition Source, Assignment[] SetOperations, ScriptNode Where, bool Lazy, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.source?.SetParent(this);

			this.setOperations = SetOperations;
			this.setOperations?.SetParent(this);

			this.where = Where;
			this.where?.SetParent(this);

			this.lazy = Lazy;
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node asynchronously, using the variables provided in 
		/// the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IDataSource Source = await this.source.GetSource(Variables);
			IResultSetEnumerator e = await Source.Find(0, int.MaxValue, false, this.where, Variables, Array.Empty<KeyValuePair<VariableReference, bool>>(), this);
			ChunkedList<object> ToUpdate = new ChunkedList<object>();
			int Count = 0;

			if (!(this.properties is null))
				this.properties.ContextVariables = Variables;

			while (await e.MoveNextAsync())
			{
				if (this.properties is null)
					this.properties = new ObjectProperties(e.Current, Variables, false);
				else
					this.properties.Object = e.Current;

				foreach (Assignment SetOperation in this.setOperations)
					SetOperation.Evaluate(this.properties);

				ToUpdate.Add(e.Current);
				Count++;
			}

			await Source.Update(this.lazy, ToUpdate);

			return new DoubleNumber(Count);
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!(SetOperation?.ForAllChildNodes(Callback, State, Order) ?? true))
						return false;
				}

				if (!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			ScriptNode NewNode;
			bool b;

			if (!(this.source is null))
			{
				b = !Callback(this.source, out NewNode, State);
				if (!(NewNode is null) && NewNode is SourceDefinition Source2)
				{
					this.source = Source2;
					this.source.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.source.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			int i, c = this.setOperations.Length;
			for (i = 0; i < c; i++)
			{
				ScriptNode SetOperation = this.setOperations[i];

				if (!(SetOperation is null))
				{
					b = !Callback(SetOperation, out NewNode, State);
					if (!(NewNode is null) && SetOperation is Assignment Assignment)
					{
						SetOperation = this.setOperations[i] = Assignment;
						Assignment.SetParent(this);
					}

					if (b || (Order == SearchMethod.TreeOrder && !SetOperation.ForAllChildNodes(Callback, State, Order)))
						return false;
				}
			}

			if (!(this.where is null))
			{
				b = !Callback(this.where, out NewNode, State);
				if (!(NewNode is null))
				{
					this.where = NewNode;
					this.where.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.where.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				foreach (Assignment SetOperation in this.setOperations)
				{
					if (!(SetOperation?.ForAllChildNodes(Callback, State, Order) ?? true))
						return false;
				}

				if (!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			int i, c;

			if (!(obj is Update O &&
				AreEqual(this.source, O.source) &&
				(c = this.setOperations.Length) == O.setOperations.Length &&
				AreEqual(this.where, O.where) &&
				base.Equals(obj)))
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!AreEqual(this.setOperations[i], O.setOperations[i]))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);

			foreach (Assignment SetOperation in this.setOperations)
				Result ^= Result << 5 ^ GetHashCode(SetOperation);

			Result ^= Result << 5 ^ GetHashCode(this.where);
			return Result;
		}


	}
}
