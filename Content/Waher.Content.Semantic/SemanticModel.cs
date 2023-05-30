using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Abstract base class for semantic models.
	/// </summary>
	public abstract class SemanticModel : ISemanticModel
	{
		/// <summary>
		/// Triples in model
		/// </summary>
		protected readonly List<ISemanticTriple> triples = new List<ISemanticTriple>();

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text content of Turtle document.</param>
		public SemanticModel()
		{
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}
	}
}
