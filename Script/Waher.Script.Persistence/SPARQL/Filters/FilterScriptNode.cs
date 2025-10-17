using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Filters
{
	/// <summary>
	/// Makes a script node, a filter node.
	/// </summary>
	public class FilterScriptNode : IFilterNode
	{
		private readonly ScriptNode node;

		/// <summary>
		/// Makes a script node, a filter node.
		/// </summary>
		/// <param name="Node">Script node.</param>
		public FilterScriptNode(ScriptNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="Cube">Semantic cube being processed.</param>
		/// <param name="Query">Query being processed.</param>
		/// <param name="Possibility">Current possibility being evaluated.</param>
		/// <returns>Result.</returns>
		public Task<IElement> EvaluateAsync(Variables Variables, ISemanticCube Cube,
			SparqlQuery Query, Possibility Possibility)
		{
			return this.node.EvaluateAsync(Variables);
		}

		/// <summary>
		/// Reference to the underlying script node.
		/// </summary>
		public ScriptNode ScriptNode => this.node;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is FilterScriptNode Typed &&
				this.node.Equals(Typed.node);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.node.GetHashCode();
		}
	}
}
