using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Static class for extracting semantic graph information from semantic models.
	/// </summary>
	public static class SemanticGraphs
	{
		/// <summary>
		/// Creates an in-memory semantic cube from a semantic model.
		/// </summary>
		/// <param name="Model">Model</param>
		/// <returns>Cube</returns>
		public static Task<InMemorySemanticCube> CreateInMemoryCube(ISemanticModel Model)
		{
			return InMemorySemanticCube.Create(Model);
		}
	}
}
