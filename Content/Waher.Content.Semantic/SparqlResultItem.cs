namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains an item in a record from the results of a SPARQL query.
	/// </summary>
	public class SparqlResultItem
	{
		/// <summary>
		/// Contains an item in a record from the results of a SPARQL query.
		/// </summary>
		/// <param name="Name">Name of item in record.</param>
		/// <param name="Value">Value of item in record.</param>
		public SparqlResultItem(string Name, ISemanticElement Value)
		{
			this.Name = Name;
			this.Value = Value;
		}

		/// <summary>
		/// Name of item in record.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Value of item in record.
		/// </summary>
		public ISemanticElement Value { get; }
	}
}
