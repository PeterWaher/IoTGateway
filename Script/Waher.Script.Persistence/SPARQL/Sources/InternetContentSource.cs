using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Runtime.Inventory;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Graph source on the Internet.
	/// </summary>
	public class InternetContentSource : IGraphSource
	{
		/// <summary>
		/// Graph source on the Internet.
		/// </summary>
		public InternetContentSource()
		{
		}

		/// <summary>
		/// How well a source with a given URI can be loaded by the class.
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <returns>How well the class supports loading the graph.</returns>
		public Grade Supports(Uri Source)
		{
			if (!Source.IsAbsoluteUri ||
				!InternetContent.CanGet(Source, out Grade _, out _))
			{
				return Grade.NotAtAll;
			}
			else
				return Grade.Barely;
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
			try
			{
				object Result = await InternetContent.GetAsync(Source,
					new KeyValuePair<string, string>("Accept", "text/turtle, application/x-turtle, application/rdf+xml;q=0.9"));

				if (Result is ISemanticCube Cube)
					return Cube;

				if (Result is ISemanticModel Model)
					return await InMemorySemanticCube.Create(Model);
			}
			catch (Exception ex)
			{
				if (NullIfNotFound)
					return null;
				else
					ExceptionDispatchInfo.Capture(ex).Throw();
			}

			if (NullIfNotFound)
				return null;
			else
				throw new ScriptRuntimeException("Graph not a semantic cube or semantic model: " + Source.ToString(), Node);
		}
	}
}
