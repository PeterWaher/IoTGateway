using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Runtime.Inventory;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Graph source in the local graph store.
	/// </summary>
	public class GraphStoreSource : IGraphSource
	{
		private readonly GraphReference reference;

		/// <summary>
		/// Graph source in the local graph store.
		/// </summary>
		public GraphStoreSource(GraphReference Reference)
		{
			this.reference = Reference;
		}

		/// <summary>
		/// How well a source with a given URI can be loaded by the class.
		/// </summary>
		/// <param name="_">Source URI</param>
		/// <returns>How well the class supports loading the graph.</returns>
		public Grade Supports(Uri _)
		{
			return Grade.NotAtAll;  // Explicitly selected by processor.
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <returns>Graph, if found, null if processed.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri Source, ScriptNode Node, bool NullIfNotFound)
		{
			string[] Files = Directory.GetFiles(this.reference.Folder, "*.*", SearchOption.TopDirectoryOnly);
			ISemanticCube Result = null;
			InMemorySemanticCube Union = null;

			foreach (string FileName in Files)
			{
				string Extension = Path.GetExtension(FileName);
				string ContentType = InternetContent.GetContentType(Extension);
				if (string.IsNullOrEmpty(ContentType) || ContentType == "application/octet-stream")
					continue;

				byte[] Bin = await Resources.ReadAllBytesAsync(FileName);
				object Decoded = await InternetContent.DecodeAsync(ContentType, Bin, Source);

				if (!(Result is null))
				{
					Union = new InMemorySemanticCube();
					await Union.Add(Result);
				}

				if (Union is null)
				{
					Result = Decoded as ISemanticCube;
					if (Result is null)
					{
						if (Decoded is ISemanticModel Model)
							Result = await InMemorySemanticCube.Create(Model);
						else
							continue;
					}
				}
				else
				{
					if (Decoded is ISemanticModel Model)
						await Union.Add(Model);
				}
			}

			return Union ?? Result ?? (NullIfNotFound ? null : new InMemorySemanticCube());
		}

	}
}
