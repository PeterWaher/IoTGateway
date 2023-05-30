namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic triples.
	/// </summary>
	public interface ISemanticTriple
	{
		/// <summary>
		/// Subject element
		/// </summary>
		ISemanticElement Subject { get; }

		/// <summary>
		/// Predicate element
		/// </summary>
		ISemanticElement Predicate { get; }

		/// <summary>
		/// Object element
		/// </summary>
		ISemanticElement Object { get; }
	}
}
