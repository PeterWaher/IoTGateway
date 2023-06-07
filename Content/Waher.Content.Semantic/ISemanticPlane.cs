using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic planes.
	/// </summary>
	public interface ISemanticPlane : ISemanticModel
	{
		/// <summary>
		/// Gets available triples in the plane, having a given X-coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByX(ISemanticElement X);

		/// <summary>
		/// Gets available triples in the plane, having a given Y-coordinate.
		/// </summary>
		/// <param name="Y">Y-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByY(ISemanticElement Y);

		/// <summary>
		/// Gets available triples in the plane, having a given X and Y-coordinate.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<IEnumerable<ISemanticTriple>> GetTriplesByXAndY(ISemanticElement X, ISemanticElement Y);

		/// <summary>
		/// Gets an enumerator of all elements along the X-axis.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetXAxisEnumerator();

		/// <summary>
		/// Gets an enumerator of all elements along the Y-axis.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetYAxisEnumerator();
	}
}
