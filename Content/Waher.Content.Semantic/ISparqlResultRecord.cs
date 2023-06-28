using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for result records of a SPARQL query.
	/// </summary>
	public interface ISparqlResultRecord : IEnumerable<SparqlResultItem>
	{
		/// <summary>
		/// Gets the value of a variable in the record. If the variable is not found,
		/// null is returned.
		/// </summary>
		/// <param name="VariableName">Name of variable.</param>
		/// <returns>Result, if found, null if not found.</returns>
		ISemanticElement this[string VariableName] { get; }
	}
}
