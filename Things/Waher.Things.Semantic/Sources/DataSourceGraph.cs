using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL;

namespace Waher.Things.Semantic.Sources
{
	/// <summary>
	/// Makes harmonized data sources and nodes available as semantic information.
	/// </summary>
	public class DataSourceGraph : IGraphSource
	{
		/// <summary>
		/// http://purl.org/dc/terms/
		/// </summary>
		public static readonly Uri DublinCoreTerms = new Uri("http://purl.org/dc/terms/");

		/// <summary>
		/// http://purl.org/dc/terms/type
		/// </summary>
		public static readonly Uri DublinCoreTermsType = new Uri(DublinCoreTerms, "type");

		/// <summary>
		/// http://purl.org/dc/terms/created
		/// </summary>
		public static readonly Uri DublinCoreTermsCreated = new Uri(DublinCoreTerms, "created");

		/// <summary>
		/// http://purl.org/dc/terms/updated
		/// </summary>
		public static readonly Uri DublinCoreTermsUpdated = new Uri(DublinCoreTerms, "updated");

		/// <summary>
		/// http://purl.org/dc/terms/creator
		/// </summary>
		public static readonly Uri DublinCoreTermsCreator = new Uri(DublinCoreTerms, "creator");

		/// <summary>
		/// http://purl.org/dc/terms/contributor
		/// </summary>
		public static readonly Uri DublinCoreTermsContributor = new Uri(DublinCoreTerms, "contributor");

		/// <summary>
		/// http://purl.org/dc/dcmitype/
		/// </summary>
		public static readonly Uri DublinCoreMetadataInitiativeType = new Uri("http://purl.org/dc/dcmitype/");

		/// <summary>
		/// http://purl.org/dc/dcmitype/Dataset
		/// </summary>
		public static readonly Uri DublinCoreMetadataInitiativeTypeDataset = new Uri(DublinCoreMetadataInitiativeType, "Dataset");

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#
		/// </summary>
		public static readonly Uri RdfSchema = new Uri("http://www.w3.org/2000/01/rdf-schema#");

		/// <summary>
		/// http://www.w3.org/2000/01/rdf-schema#label
		/// </summary>
		public static readonly Uri RdfSchemaLabel = new Uri(RdfSchema, "label");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#
		/// </summary>
		public static readonly Uri XmlSchema = new Uri("http://www.w3.org/2001/XMLSchema#");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:
		/// </summary>
		public static readonly Uri IotConcentrator = new Uri("urn:ieee:iot:concentrator:1.0:");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:DataSource
		/// </summary>
		public static readonly Uri IotConcentratorDataSource = new Uri(IotConcentrator, "DataSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:childSource
		/// </summary>
		public static readonly Uri IotConcentratorChildSource = new Uri(IotConcentrator, "childSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:rootNode
		/// </summary>
		public static readonly Uri IotConcentratorRootNode = new Uri(IotConcentrator, "rootNode");

		/// <summary>
		/// Makes harmonized data sources and nodes available as semantic information.
		/// </summary>
		public DataSourceGraph()
		{
		}

		/// <summary>
		/// How well the source handles a given Graph URI.
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <returns>How well the URI is supported.</returns>
		public Grade Supports(Uri GraphUri)
		{
			if (!IsServerDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return Grade.NotAtAll;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return Grade.NotAtAll;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2 || !Gateway.ConcentratorServer.TryGetDataSource(Parts[1], out _))
				return Grade.NotAtAll;

			if (c > 4)
				return Grade.NotAtAll;

			return Grade.Excellent;
		}

		/// <summary>
		/// Checks if a domain is the server domain, or optionally, an alternative domain.
		/// </summary>
		/// <param name="Domain">Domain to check.</param>
		/// <param name="IncludeAlternativeDomains">If alternative domains are to be checked as well.</param>
		/// <returns>If the domain to check is the server domain, or optionally, an alternative domain.</returns>
		public static bool IsServerDomain(CaseInsensitiveString Domain, bool IncludeAlternativeDomains)
		{
			if (Domain == Gateway.Domain || (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain) && Domain == "localhost"))
				return true;

			if (IncludeAlternativeDomains)
			{
				foreach (CaseInsensitiveString s in Gateway.AlternativeDomains)
				{
					if (s == Domain)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="GraphUri">Graph URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri GraphUri, ScriptNode Node, bool NullIfNotFound)
		{
			if (!IsServerDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return null;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c <= 1 || !string.IsNullOrEmpty(Parts[0]))
				return null;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c <= 2 || !Gateway.ConcentratorServer.TryGetDataSource(Parts[1], out IDataSource Source))
				return null;

			InMemorySemanticCube Result = new InMemorySemanticCube();
			Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

			switch (c)
			{
				case 2: // DOMAIN/Source
					await AppendSourceInformation(Result, Source, Language);
					break;

				case 3: // /DOMAIN/Source/NodeID
					break;

				case 4: // /DOMAIN/Source/Partition/NodeID
					break;

				default:
					return null;
			}

			return null;
		}

		public static async Task AppendSourceInformation(InMemorySemanticCube Result, 
			IDataSource Source, Language Language)
		{
			// TODO: Check access rights

			string SourcePath = "/" + HttpUtility.UrlEncode(Source.SourceID);
			UriNode GraphUriNode = new UriNode(new Uri(Gateway.GetUrl(SourcePath)));

			Result.Add(new SemanticTriple(
				GraphUriNode,
				new UriNode(DublinCoreTermsType),
				new UriNode(IotConcentratorDataSource)));

			Result.Add(new SemanticTriple(
				GraphUriNode,
				new UriNode(RdfSchemaLabel),
				new StringLiteral(await Source.GetNameAsync(Language))));

			Result.Add(new SemanticTriple(
				GraphUriNode,
				new UriNode(DublinCoreTermsUpdated),
				new DateTimeLiteral(Source.LastChanged)));

			if (Source.HasChildren)
			{
				foreach (IDataSource ChildSource in Source.ChildSources)
				{
					Result.Add(new SemanticTriple(
						GraphUriNode,
						new UriNode(IotConcentratorChildSource),
						new UriNode(new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(ChildSource.SourceID))))));
				}
			}

			IEnumerable<INode> RootNodes = Source.RootNodes;

			if (!(RootNodes is null))
			{
				foreach (INode RootNode in RootNodes)
				{
					Result.Add(new SemanticTriple(
						GraphUriNode,
						new UriNode(IotConcentratorRootNode),
						new UriNode(new Uri(Gateway.GetUrl(SourcePath + "/" + HttpUtility.UrlEncode(RootNode.NodeId))))));
				}
			}
		}

	}
}
