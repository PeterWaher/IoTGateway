using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things.Semantic.Ontologies;

namespace Waher.Things.Semantic.Sources.DynamicGraphs
{
	/// <summary>
	/// Dynamic graph of a data source on the Neuron.
	/// </summary>
	public class SourceGraph : IDynamicGraph
	{
		private readonly IDataSource source;

		/// <summary>
		/// Dynamic graph of a data source on the Neuron.
		/// </summary>
		/// <param name="Source">Data source.</param>
		public SourceGraph(IDataSource Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Generates the semantic graph.
		/// </summary>
		/// <param name="Result">Result set will be output here.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Caller">Origin of request.</param>
		public Task GenerateGraph(InMemorySemanticCube Result, Language Language, RequestOrigin Caller)
		{
			return GenerateGraph(Result, Language, this.source);
		}

		/// <summary>
		/// Generates a semantic graph of a data source.
		/// </summary>
		/// <param name="Result">Result set will be output here.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Source">Data source.</param>
		public static async Task GenerateGraph(InMemorySemanticCube Result, Language Language, 
			IDataSource Source)
		{ 
			UriNode SourceGraphUriNode = new UriNode(DataSourceGraph.GetSourceUri(Source.SourceID));

			Result.Add(SourceGraphUriNode, Rdf.type, IoTConcentrator.DataSource);
			Result.Add(SourceGraphUriNode, IoTConcentrator.sourceId, Source.SourceID);
			Result.Add(SourceGraphUriNode, RdfSchema.label, await Source.GetNameAsync(Language));
			Result.Add(SourceGraphUriNode, DublinCoreTerms.updated, Source.LastChanged);
			Result.Add(SourceGraphUriNode, IoTConcentrator.hasChildSources, Source.HasChildren);

			ChunkedList<ISemanticElement> Items = new ChunkedList<ISemanticElement>();

			if (Source.HasChildren)
			{
				foreach (IDataSource ChildSource in Source.ChildSources)
					Items.Add(new UriNode(DataSourceGraph.GetSourceUri(ChildSource.SourceID)));

				Result.AddContainer(SourceGraphUriNode, Rdf.Bag, IoTConcentrator.childSources,
					Items);

				Items.Clear();
			}

			IEnumerable<INode> RootNodes = Source.RootNodes;

			if (!(RootNodes is null))
			{
				foreach (INode RootNode in RootNodes)
					Items.Add(new UriNode(DataSourceGraph.GetNodeUri(RootNode)));

				Result.AddContainer(SourceGraphUriNode, Rdf.Bag, IoTConcentrator.rootNodes, Items);
			}
		}
	}
}
