using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic cube.
	/// </summary>
	public class InMemorySemanticLine : ISemanticLine
	{
		private readonly LinkedList<ISemanticTriple> elements = new LinkedList<ISemanticTriple>();
		private readonly SortedDictionary<ISemanticElement, LinkedList<ISemanticTriple>> points = new SortedDictionary<ISemanticElement, LinkedList<ISemanticTriple>>();
		private readonly ISemanticElement reference;
		private LinkedList<ISemanticTriple> last = null;

		/// <summary>
		/// In-memory semantic cube.
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
			this.elements.AddLast(Triple);

			if ((this.last is null || !this.last.First.Value.Equals(Point)) &&
				!this.points.TryGetValue(Point, out this.last))
			{
				this.last = new LinkedList<ISemanticTriple>();
				this.points[Point] = this.last;
			}

			this.last.AddLast(Triple);
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
		/// Gets available triples on the line, having a given coordinate.
		/// </summary>
		/// <param name="X">Coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement X)
		{
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
			return Task.FromResult<IEnumerator<ISemanticElement>>(this.points.Keys.GetEnumerator());
		}
	}
}
