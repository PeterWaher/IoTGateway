using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic lines.
	/// </summary>
	public interface ISemanticLine : IEnumerable<ISemanticTriple>
	{
		/// <summary>
		/// Gets available triples on the line, having a given coordinate.
		/// </summary>
		/// <param name="X">Coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement X);

		/// <summary>
		/// Gets an enumerator of all values along the line.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetValueEnumerator();
	}
}
