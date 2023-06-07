using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic model.
	/// </summary>
	public class InMemorySemanticModel : ISemanticModel
	{
		/// <summary>
		/// Triples in model.
		/// </summary>
		protected readonly LinkedList<ISemanticTriple> triples;

		/// <summary>
		/// In-memory semantic model.
		/// </summary>
		public InMemorySemanticModel()
		{
			this.triples = new LinkedList<ISemanticTriple>();
		}

		/// <summary>
		/// In-memory semantic model.
		/// </summary>
		/// <param name="Triples">Triples.</param>
		public InMemorySemanticModel(IEnumerable<ISemanticTriple> Triples)
		{
			this.triples = Triples as LinkedList<ISemanticTriple>;

			if (this.triples is null)
			{
				this.triples = new LinkedList<ISemanticTriple>();

				foreach (ISemanticTriple Triple in Triples)
					this.triples.AddLast(Triple);
			}
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}
	}
}
