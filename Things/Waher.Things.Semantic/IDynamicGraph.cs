using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Language;

namespace Waher.Things.Semantic
{
	/// <summary>
	/// Interface for dynamic graphs.
	/// </summary>
	public interface IDynamicGraph
	{
		/// <summary>
		/// Generates the semantic graph.
		/// </summary>
		/// <param name="Result">Result set will be output here.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Caller">Origin of request.</param>
		Task GenerateGraph(InMemorySemanticCube Result, Language Language, RequestOrigin Caller);
	}
}
