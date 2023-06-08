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
		/// <param name="Index">Column index.</param>
		public SparqlResultItem(string Name, ISemanticElement Value, int Index)
		{
			this.Name = Name;
			this.Value = Value;
			this.Index = Index;
		}

		/// <summary>
		/// Name of item in record.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Value of item in record.
		/// </summary>
		public ISemanticElement Value { get; }

		/// <summary>
		/// Column index.
		/// </summary>
		public int Index { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + "=" + this.Value.ToString();
		}
	}
}
