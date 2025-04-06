using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic plane.
	/// </summary>
	public class InMemorySemanticPlane : ISemanticPlane
	{
		private readonly ChunkedList<Tuple<ISemanticElement, ISemanticElement, ISemanticTriple>> elements = new ChunkedList<Tuple<ISemanticElement, ISemanticElement, ISemanticTriple>>();
		private readonly ISemanticElement reference;
		private SortedDictionary<ISemanticElement, InMemorySemanticLine> yPerX = null;
		private SortedDictionary<ISemanticElement, InMemorySemanticLine> xPerY = null;

		/// <summary>
		/// In-memory semantic plane.
		/// </summary>
		/// <param name="Reference">Reference</param>
		public InMemorySemanticPlane(ISemanticElement Reference)
		{
			this.reference = Reference;
		}

		/// <summary>
		/// Plane reference.
		/// </summary>
		public ISemanticElement Reference => this.reference;

		/// <summary>
		/// Adds an element to the plane.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Triple">Triple</param>
		public void Add(ISemanticElement X, ISemanticElement Y, ISemanticTriple Triple)
		{
			this.elements.Add(new Tuple<ISemanticElement, ISemanticElement, ISemanticTriple>(X, Y, Triple));
			this.xPerY = null;
			this.yPerX = null;
		}

		/// <summary>
		/// Adds a set of triples to the plane.
		/// </summary>
		/// <param name="Triples">Enumerable set of triples.</param>
		/// <param name="XIndex">X-coordinate index.</param>
		/// <param name="YIndex">Y-coordinate index.</param>
		public void Add(IEnumerable<ISemanticTriple> Triples, int XIndex, int YIndex)
		{
			if (!(Triples is null))
			{
				foreach (ISemanticTriple Triple in Triples)
					this.Add(Triple[XIndex], Triple[YIndex], Triple);
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
			private readonly IEnumerator<Tuple<ISemanticElement, ISemanticElement, ISemanticTriple>> e;

			public TripleEnumerator(IEnumerator<Tuple<ISemanticElement, ISemanticElement, ISemanticTriple>> e)
			{
				this.e = e;
			}

			public ISemanticTriple Current => this.e.Current.Item3;
			object IEnumerator.Current => this.e.Current.Item3;
			public void Dispose() => this.e.Dispose();
			public bool MoveNext() => this.e.MoveNext();
			public void Reset() => this.e.Reset();
		}

		/// <summary>
		/// Gets available triples in the plane, having a given X-coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByX(ISemanticElement X)
		{
			this.CheckXOrdered();

			if (!this.yPerX.TryGetValue(X, out InMemorySemanticLine Line))
				Line = null;

			return Task.FromResult<ISemanticLine>(Line);
		}

		/// <summary>
		/// Gets available triples in the plane, having a given Y-coordinate.
		/// </summary>
		/// <param name="Y">Y-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByY(ISemanticElement Y)
		{
			this.CheckYOrdered();

			if (!this.xPerY.TryGetValue(Y, out InMemorySemanticLine Line))
				Line = null;

			return Task.FromResult<ISemanticLine>(Line);
		}

		/// <summary>
		/// Gets available triples in the plane, having a given X and Y-coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<IEnumerable<ISemanticTriple>> GetTriplesByXAndY(ISemanticElement X, ISemanticElement Y)
		{
			this.CheckXOrdered();

			if (!this.yPerX.TryGetValue(X, out InMemorySemanticLine Line))
				return Task.FromResult<IEnumerable<ISemanticTriple>>(null);

			return Line.GetTriples(Y);
		}

		/// <summary>
		/// Gets an enumerator of all elements along the X-axis.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetXAxisEnumerator()
		{
			this.CheckXOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.yPerX.Keys.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator of all elements along the Y-axis.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetYAxisEnumerator()
		{
			this.CheckYOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.xPerY.Keys.GetEnumerator());
		}

		private void CheckXOrdered()
		{
			if (this.yPerX is null)
			{
				SortedDictionary<ISemanticElement, InMemorySemanticLine> Ordered =
					new SortedDictionary<ISemanticElement, InMemorySemanticLine>();
				ISemanticElement LastPoint = null;
				InMemorySemanticLine Last = null;

				foreach (Tuple<ISemanticElement, ISemanticElement, ISemanticTriple> P in this.elements)
				{
					if ((LastPoint is null || !LastPoint.Equals(P.Item1)) &&
						!Ordered.TryGetValue(P.Item1, out Last))
					{
						Last = new InMemorySemanticLine(P.Item1, P.Item2);
						Ordered[P.Item1] = Last;
					}

					Last.Add(P.Item2, P.Item3);
				}

				this.yPerX = Ordered;
			}
		}

		private void CheckYOrdered()
		{
			if (this.xPerY is null)
			{
				SortedDictionary<ISemanticElement, InMemorySemanticLine> Ordered =
					new SortedDictionary<ISemanticElement, InMemorySemanticLine>();
				ISemanticElement LastPoint = null;
				InMemorySemanticLine Last = null;

				foreach (Tuple<ISemanticElement, ISemanticElement, ISemanticTriple> P in this.elements)
				{
					if ((LastPoint is null || !LastPoint.Equals(P.Item2)) &&
						!Ordered.TryGetValue(P.Item2, out Last))
					{
						Last = new InMemorySemanticLine(P.Item2, P.Item1);
						Ordered[P.Item2] = Last;
					}

					Last.Add(P.Item1, P.Item3);
				}

				this.xPerY = Ordered;
			}
		}
	}
}
