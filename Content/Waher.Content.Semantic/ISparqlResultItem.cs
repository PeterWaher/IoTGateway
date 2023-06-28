namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for items in a record from the results of a SPARQL query.
	/// </summary>
	public interface ISparqlResultItem
	{
		/// <summary>
		/// Name of item in record.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Value of item in record.
		/// </summary>
		ISemanticElement Value { get; set; }
	}
}
