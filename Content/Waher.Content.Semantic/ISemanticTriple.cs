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

		/// <summary>
		/// Access to elements: 0=Subject, 1=Predicate, 2=Object.
		/// </summary>
		/// <param name="Index">0=Subject, 1=Predicate, 2=Object</param>
		/// <returns>Corresponding semantic element.</returns>
		ISemanticElement this[int Index]
		{
			get;
		}
	}
}
