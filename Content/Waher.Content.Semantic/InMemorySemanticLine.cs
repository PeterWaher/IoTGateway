using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic line.
	/// </summary>
	public class InMemorySemanticLine : ISemanticLine
	{
		private readonly LinkedList<KeyValuePair<ISemanticElement, ISemanticTriple>> elements = new LinkedList<KeyValuePair<ISemanticElement, ISemanticTriple>>();
		private readonly ISemanticElement reference;
		private SortedDictionary<ISemanticElement, LinkedList<ISemanticTriple>> points = null;

		/// <summary>
		/// In-memory semantic line.
		/// </summary>
		/// <param name="Reference">Reference</param>
		public InMemorySemanticLine(ISemanticElement Reference)
		{
			this.reference = Reference;
		}

		/// <summary>
		/// Line reference.
		/// </summary>
		public ISemanticElement Reference => this.reference;

		/// <summary>
		/// Adds an element to the line.
		/// </summary>
		/// <param name="Point">Coordinate.</param>
		/// <param name="Triple">Triple</param>
		internal void Add(ISemanticElement Point, ISemanticTriple Triple)
		{
			this.points = null;
			this.elements.AddLast(new KeyValuePair<ISemanticElement, ISemanticTriple>(Point, Triple));
		}

		/// <summary>
		/// Adds a set of triples to the plane.
		/// </summary>
		/// <param name="Triples">Enumerable set of triples.</param>
		/// <param name="ZIndex">Z-coordinate index.</param>
		internal void Add(IEnumerable<ISemanticTriple> Triples, int ZIndex)
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

			if (!this.points.TryGetValue(X, out LinkedList<ISemanticTriple> Points))
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
				SortedDictionary<ISemanticElement, LinkedList<ISemanticTriple>> Ordered = 
					new SortedDictionary<ISemanticElement, LinkedList<ISemanticTriple>>();
				ISemanticElement LastPoint = null;
				LinkedList<ISemanticTriple> Last = null;

				foreach (KeyValuePair<ISemanticElement, ISemanticTriple> P in this.elements)
				{
					if ((LastPoint is null || !LastPoint.Equals(P.Key)) &&
						!Ordered.TryGetValue(P.Key, out Last))
					{
						Last = new LinkedList<ISemanticTriple>();
						Ordered[P.Key] = Last;
					}

					Last.AddLast(P.Value);
				}

				this.points = Ordered;
			}
		}
	}
}
