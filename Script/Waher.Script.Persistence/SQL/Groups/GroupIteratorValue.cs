using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Groups
{
	/// <summary>
	/// Returns the result value from an iterator.
	/// </summary>
	public class GroupIteratorValue : ScriptNode
	{
		private readonly IIterativeEvaluator iterator;

		/// <summary>
		/// Returns the result value from an iterator.
		/// </summary>
		/// <param name="Iterator">Iterator.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GroupIteratorValue(IIterativeEvaluator Iterator,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.iterator = Iterator;
		}

		/// <summary>
		/// Iterator
		/// </summary>
		public IIterativeEvaluator Iterator => this.iterator;

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			return true;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.iterator.GetAggregatedResult();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Iterator(" + base.ToString() + ")";
		}
	}
}
