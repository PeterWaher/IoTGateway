using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Executes an DROP INDEX ... ON ... statement against the object database.
	/// </summary>
	public class DropCollection : ScriptNode, IEvaluateAsync
	{
		private SourceDefinition source;

		/// <summary>
		/// Executes an DROP COLLECTION|TABLE ... statement against the object database.
		/// </summary>
		/// <param name="Source">Source to create index in.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DropCollection(SourceDefinition Source, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.source?.SetParent(this);
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
			await Source.DropCollection();
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
			}

			bool b;

			if (!(this.source is null))
			{
				b = !Callback(this.source, out ScriptNode NewNode, State);
				if (!(NewNode is null) && NewNode is SourceDefinition Source2)
				{
					this.source = Source2;
					this.source.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.source.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is DropCollection O &&
				AreEqual(this.source, O.source) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			return Result;
		}

	}
}
