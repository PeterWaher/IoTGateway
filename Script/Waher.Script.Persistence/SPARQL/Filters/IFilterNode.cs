using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Filters
{
	/// <summary>
	/// Interface for filter nodes.
	/// </summary>
	public interface IFilterNode
	{
		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="Cube">Semantic cube being processed.</param>
		/// <param name="Query">Query being processed.</param>
		/// <param name="Possibility">Current possibility being evaluated.</param>
		/// <returns>Result.</returns>
		Task<IElement> EvaluateAsync(Variables Variables, ISemanticCube Cube, 
			SparqlQuery Query, Possibility Possibility);

		/// <summary>
		/// Reference to the underlying script node.
		/// </summary>
		ScriptNode ScriptNode { get; }
	}
}
