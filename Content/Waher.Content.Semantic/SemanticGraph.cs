using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains triples that form a graph.
	/// </summary>
	public class SemanticGraph : ISemanticModel
	{
		private readonly LinkedList<ISemanticTriple> triples = new LinkedList<ISemanticTriple>();
		private readonly Dictionary<ISemanticElement, bool> nodes = new Dictionary<ISemanticElement, bool>();
		private ISemanticElement lastSubject = null;
		private ISemanticElement[] nodesStatic = null;

		/// <summary>
		/// Contains triples that form a graph.
		/// </summary>
		public SemanticGraph()
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

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Triple">Triple</param>
		public void Add(ISemanticTriple Triple)
		{
			this.triples.AddLast(Triple);

			if (this.lastSubject is null || !this.lastSubject.Equals(Triple.Subject))
			{
				this.nodes[Triple.Subject] = true;
				this.lastSubject = Triple.Subject;
				this.nodesStatic = null;
			}

			if (!Triple.Object.IsLiteral)
			{
				this.nodes[Triple.Object] = true;
				this.nodesStatic = null;
			}
		}

		/// <summary>
		/// Nodes in graph.
		/// </summary>
		public ISemanticElement[] Nodes
		{
			get
			{
				if (this.nodesStatic is null)
				{
					ISemanticElement[] Result = new ISemanticElement[this.nodes.Count];
					this.nodes.Keys.CopyTo(Result, 0);
					this.nodesStatic = Result;
				}

				return this.nodesStatic;
			}
		}
	}
}
