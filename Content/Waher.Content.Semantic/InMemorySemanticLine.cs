using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic line.
	/// </summary>
	public class InMemorySemanticLine : ISemanticLine
	{
		private readonly ChunkedList<KeyValuePair<ISemanticElement, ISemanticTriple>> elements = new ChunkedList<KeyValuePair<ISemanticElement, ISemanticTriple>>();
		private readonly ISemanticElement reference1;
		private readonly ISemanticElement reference2;
		private SortedDictionary<ISemanticElement, ChunkedList<ISemanticTriple>> points = null;

		/// <summary>
		/// In-memory semantic line.
		/// </summary>
		/// <param name="Reference1">Reference 1</param>
		/// <param name="Reference2">Reference 2</param>
		public InMemorySemanticLine(ISemanticElement Reference1, ISemanticElement Reference2)
		{
			this.reference1 = Reference1;
			this.reference2 = Reference2;
		}

		/// <summary>
		/// Line reference 1.
		/// </summary>
		public ISemanticElement Reference1 => this.reference1;

		/// <summary>
		/// Line reference 2.
		/// </summary>
		public ISemanticElement Reference2 => this.reference2;

		/// <summary>
		/// Adds an element to the line.
		/// </summary>
		/// <param name="Point">Coordinate.</param>
		/// <param name="Triple">Triple</param>
		public void Add(ISemanticElement Point, ISemanticTriple Triple)
		{
			this.points = null;
			this.elements.Add(new KeyValuePair<ISemanticElement, ISemanticTriple>(Point, Triple));
		}

		/// <summary>
		/// Adds a set of triples to the plane.
		/// </summary>
		/// <param name="Triples">Enumerable set of triples.</param>
		/// <param name="ZIndex">Z-coordinate index.</param>
		public void Add(IEnumerable<ISemanticTriple> Triples, int ZIndex)
		{
			if (!(Triples is null))
			{
				foreach (ISemanticTriple Triple in Triples)
					this.Add(Triple[ZIndex], Triple);
			}
		}

		/// <summary>
		/// Gets an enumerator over all elements in plance.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return new TripleEnumerator(this.elements.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator over all elements in plance.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new TripleEnumerator(this.elements.GetEnumerator());
		}

		private class TripleEnumerator : IEnumerator<ISemanticTriple>
		{
			private readonly IEnumerator<KeyValuePair<ISemanticElement, ISemanticTriple>> e;

			public TripleEnumerator(IEnumerator<KeyValuePair<ISemanticElement, ISemanticTriple>> e)
			{
				this.e = e;
			}

			public ISemanticTriple Current => this.e.Current.Value;
			object IEnumerator.Current => this.e.Current.Value;
			public void Dispose() => this.e.Dispose();
			public bool MoveNext() => this.e.MoveNext();
			public void Reset() => this.e.Reset();
		}

		/// <summary>
		/// Gets available triples on the line, having a given coordinate.
		/// </summary>
		/// <param name="X">Coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement X)
		{
			this.CheckOrdered();

			if (!this.points.TryGetValue(X, out ChunkedList<ISemanticTriple> Points))
				Points = null;

			return Task.FromResult<IEnumerable<ISemanticTriple>>(Points);
		}

		/// <summary>
		/// Gets an enumerator of all values along the line.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetValueEnumerator()
		{
			this.CheckOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.points.Keys.GetEnumerator());
		}

		private void CheckOrdered()
		{
			if (this.points is null)
			{
				SortedDictionary<ISemanticElement, ChunkedList<ISemanticTriple>> Ordered = 
					new SortedDictionary<ISemanticElement, ChunkedList<ISemanticTriple>>();
				ISemanticElement LastPoint = null;
				ChunkedList<ISemanticTriple> Last = null;

				foreach (KeyValuePair<ISemanticElement, ISemanticTriple> P in this.elements)
				{
					if ((LastPoint is null || !LastPoint.Equals(P.Key)) &&
						!Ordered.TryGetValue(P.Key, out Last))
					{
						Last = new ChunkedList<ISemanticTriple>();
						Ordered[P.Key] = Last;
					}

					Last.Add(P.Value);
				}

				this.points = Ordered;
			}
		}
	}
}
