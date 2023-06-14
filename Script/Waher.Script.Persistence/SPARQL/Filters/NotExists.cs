using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SPARQL.Filters
{
	/// <summary>
	/// Checks if a pattern has at least no matches.
	/// </summary>
	public class NotExists : ScriptNode, IFilterNode
	{
		private readonly SparqlPattern pattern;

		/// <summary>
		/// Checks if a pattern has at least no matches.
		/// </summary>
		/// <param name="Pattern">Pattern to check.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NotExists(SparqlPattern Pattern, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.pattern = Pattern;
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
			return BooleanValue.False;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return Task.FromResult<IElement>(BooleanValue.False);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="Cube">Semantic cube being processed.</param>
		/// <param name="VariablesProcessed">Variables already processed.</param>
		/// <param name="Query">Query being processed.</param>
		/// <param name="Possibility">Current possibility being evaluated.</param>
		/// <returns>Result.</returns>
		public async Task<IElement> EvaluateAsync(Variables Variables, ISemanticCube Cube,
			Dictionary<string, bool> VariablesProcessed, SparqlQuery Query, Possibility Possibility)
		{
			IEnumerable<Possibility> Possibilities = await this.pattern.Search(Cube, Variables, VariablesProcessed,
				new Possibility[] { Possibility }, Query);
			if (Possibilities is null)
				return BooleanValue.True;

			return Possibilities.GetEnumerator().MoveNext() ? BooleanValue.False : BooleanValue.True;
		}

		/// <summary>
		/// Reference to the underlying script node.
		/// </summary>
		public ScriptNode ScriptNode => this;

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
				if (!this.pattern.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			if (!this.pattern.ForAll(Callback, State, Order))
				return false;

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.pattern.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is NotExists O &&
				this.pattern.Equals(O.pattern) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.pattern.GetHashCode();
			return Result;
		}
	}
}
