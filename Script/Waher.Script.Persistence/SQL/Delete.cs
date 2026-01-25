using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes a DELETE statement against the object database.
	/// </summary>
	public class Delete : ScriptNode, IEvaluateAsync
	{
		private SourceDefinition source;
		private ScriptNode where;
		private readonly bool lazy;

		/// <summary>
		/// Executes a DELETE statement against the object database.
		/// </summary>
		/// <param name="Source">Source to delete from.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Delete(SourceDefinition Source, ScriptNode Where, bool Lazy, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.source?.SetParent(this);

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
			int? Count = await Source.Delete(this.lazy, 0, int.MaxValue, this.where, 
				Variables, Array.Empty<KeyValuePair<VariableReference, bool>>(), this);

			if (Count.HasValue)
				return new DoubleNumber(Count.Value);
			else
				return ObjectValue.Null;
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

				if (!(this.where?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Delete O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.where, O.where) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			return Result;
		}


	}
}
