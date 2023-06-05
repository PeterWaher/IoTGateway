using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains triples that form a graph.
	/// </summary>
	public class SemanticGraph : ISemanticModel
	{
		private readonly ISemanticModel model;
		private LinkedList<ISemanticTriple> triples;

		/// <summary>
		/// Contains triples that form a graph.
		/// </summary>
		/// <param name="Model">Existing model.</param>
		public SemanticGraph(ISemanticModel Model)
		{
			this.model = Model;
			this.triples = null;
		}

		/// <summary>
		/// Contains triples that form a graph.
		/// </summary>
		/// <param name="Model">Existing model.</param>
		public SemanticGraph()
		{
			this.model = null;
			this.triples = new LinkedList<ISemanticTriple>();
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.model?.GetEnumerator() ?? this.triples.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator for the semantic information in the document.
		/// </summary>
		/// <returns>Enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.model?.GetEnumerator() ?? this.triples.GetEnumerator();
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Triple">Triple</param>
		public void Add(ISemanticTriple Triple)
		{
			if (this.triples is null)
			{
				this.triples = new LinkedList<ISemanticTriple>();

				foreach (ISemanticTriple T in this.model)
					this.triples.AddLast(T);
			}

			this.triples.AddLast(Triple);
		}
	}
}
