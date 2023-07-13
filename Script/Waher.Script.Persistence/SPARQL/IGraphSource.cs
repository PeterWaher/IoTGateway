using System;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Inventory;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Interface for graph sources.
	/// </summary>
	public interface IGraphSource : IProcessingSupport<Uri>
	{
		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		Task<ISemanticCube> LoadGraph(Uri Source, ScriptNode Node, bool NullIfNotFound);
	}
}
