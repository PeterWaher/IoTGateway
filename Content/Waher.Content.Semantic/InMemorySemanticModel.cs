using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic model.
	/// </summary>
	public class InMemorySemanticModel : ISemanticModel
	{
		private readonly LinkedList<ISemanticTriple> elements;

		/// <summary>
		/// In-memory semantic model.
		/// </summary>
		/// <param name="Triples">Triples.</param>
		public InMemorySemanticModel(IEnumerable<ISemanticTriple> Triples)
		{
			this.elements = Triples as LinkedList<ISemanticTriple>;

			if (this.elements is null)
			{
				this.elements = new LinkedList<ISemanticTriple>();

				foreach (ISemanticTriple Triple in Triples)
					this.elements.AddLast(Triple);
			}
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}
	}
}
