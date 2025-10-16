using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic ontologies.
	/// </summary>
	public interface IOntology : IProcessingSupport<string>
	{
		/// <summary>
		/// Ontology namespace.
		/// </summary>
		string OntologyNamespace { get; }

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		string OntologyPrefix { get; }
	}
}
