using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic cube.
	/// </summary>
	public class InMemorySemanticPlane : ISemanticPlane
	{
		private readonly LinkedList<ISemanticTriple> elements = new LinkedList<ISemanticTriple>();
		private readonly SortedDictionary<ISemanticElement, InMemorySemanticLine> yPerX = new SortedDictionary<ISemanticElement, InMemorySemanticLine>();
		private readonly SortedDictionary<ISemanticElement, InMemorySemanticLine> xPerY = new SortedDictionary<ISemanticElement, InMemorySemanticLine>();
		private readonly ISemanticElement reference;
		private InMemorySemanticLine lastX = null;
		private InMemorySemanticLine lastY = null;

		/// <summary>
		/// In-memory semantic cube.
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
		internal void Add(ISemanticElement X, ISemanticElement Y, ISemanticTriple Triple)
		{
			this.elements.AddLast(Triple);

			if ((this.lastX is null || !this.lastX.Reference.Equals(X)) &&
				!this.yPerX.TryGetValue(X, out this.lastX))
			{
				this.lastX = new InMemorySemanticLine(X);
				this.yPerX[X] = this.lastX;
			}

			this.lastX.Add(Y, Triple);

			if ((this.lastY is null || !this.lastY.Reference.Equals(Y)) &&
				!this.xPerY.TryGetValue(Y, out this.lastY))
			{
				this.lastY = new InMemorySemanticLine(Y);
				this.xPerY[Y] = this.lastY;
			}

			this.lastY.Add(X, Triple);
		}

		/// <summary>
		/// Gets an enumerator over all elements in plance.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator over all elements in plance.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		/// <summary>
		/// Gets available triples in the plane, having a given X-coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByX(ISemanticElement X)
		{
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
			if (!this.yPerX.TryGetValue(X, out InMemorySemanticLine Line))
				return Task.FromResult<IEnumerable<ISemanticTriple>>(null);

			return Line.GetTriples(Y);
		}
	}
}
