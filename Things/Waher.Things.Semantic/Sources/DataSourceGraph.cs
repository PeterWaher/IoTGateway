using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL;
using Waher.Things.Semantic.Ontologies;

namespace Waher.Things.Semantic.Sources
{
	/// <summary>
	/// Makes harmonized data sources and nodes available as semantic information.
	/// </summary>
	public class DataSourceGraph : IGraphSource
	{
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
		/// <param name="Caller">Information about entity making the request.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public async Task<ISemanticCube> LoadGraph(Uri GraphUri, ScriptNode Node, bool NullIfNotFound,
			RequestOrigin Caller)
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

			if (c <= 2)
				return null;

			string SourceID = Parts[1];
			if (!Gateway.ConcentratorServer.TryGetDataSource(SourceID, out IDataSource Source))
				return null;

			if (!await Source.CanViewAsync(Caller))
				return null;

			InMemorySemanticCube Result = new InMemorySemanticCube();
			Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

			switch (c)
			{
				case 2: // DOMAIN/Source
					await AppendSourceInformation(Result, Source, Language, null);
					break;

				case 3: // /DOMAIN/Source/NodeID
					string NodeID = Parts[2];
					INode NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					await AppendNodeInformation(Result, NodeObj, Language, Caller);
					break;

				case 4: // /DOMAIN/Source/Partition/NodeID
					string Partition = Parts[2];
					NodeID = Parts[3];
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					await AppendNodeInformation(Result, NodeObj, Language, Caller);
					break;

				case 5: // /DOMAIN/Source/NodeID/Category/Action
					NodeID = Parts[2];
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					return await GetNodeActionGraph(Result, NodeObj, Language, Parts[3], Parts[4]);

				case 6: // /DOMAIN/Source/Partition/NodeID/Category/Action
					Partition = Parts[2];
					NodeID = Parts[3];
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					return await GetNodeActionGraph(Result, NodeObj, Language, Parts[4], Parts[5]);

				default:
					return null;
			}

			return null;
		}

		/// <summary>
		/// Appends semantic information about a data source to a graph.
		/// </summary>
		/// <param name="Result">Graph being generated.</param>
		/// <param name="Source">Source to append information about.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Caller; can be null if authorization has been performed at a higher level.</param>
		public static async Task AppendSourceInformation(InMemorySemanticCube Result,
			IDataSource Source, Language Language, RequestOrigin Caller)
		{
			if (Source is null)
				return;

			if (!(Caller is null) && !await Source.CanViewAsync(Caller))
				return;

			string SourcePath = "/" + HttpUtility.UrlEncode(Source.SourceID);
			UriNode SourceGraphUriNode = new UriNode(new Uri(Gateway.GetUrl(SourcePath)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(DublinCore.Terms.Type),
				new UriNode(IoTConcentrator.DataSource)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(IoTConcentrator.SourceId),
				new StringLiteral(Source.SourceID)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(RdfSchema.Label),
				new StringLiteral(await Source.GetNameAsync(Language))));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(DublinCore.Terms.Updated),
				new DateTimeLiteral(Source.LastChanged)));

			Result.Add(new SemanticTriple(
				SourceGraphUriNode,
				new UriNode(IoTConcentrator.HasChildSource),
				new BooleanLiteral(Source.HasChildren)));

			if (Source.HasChildren)
			{
				foreach (IDataSource ChildSource in Source.ChildSources)
				{
					Result.Add(new SemanticTriple(
						SourceGraphUriNode,
						new UriNode(IoTConcentrator.ChildSource),
						new UriNode(new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(ChildSource.SourceID))))));
				}
			}

			IEnumerable<INode> RootNodes = Source.RootNodes;

			if (!(RootNodes is null))
			{
				foreach (INode RootNode in RootNodes)
				{
					Result.Add(new SemanticTriple(
						SourceGraphUriNode,
						new UriNode(IoTConcentrator.RootNode),
						new UriNode(GetNodeUri(RootNode))));
				}
			}
		}

		/// <summary>
		/// Gets the Graph URI for a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <returns>Graph URI for Node Graph.</returns>
		public static Uri GetNodeUri(INode Node)
		{
			StringBuilder sb = new StringBuilder();
			GetNodeUri(Node, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="sb">URI being built.</param>
		private static void GetNodeUri(INode Node, StringBuilder sb)
		{
			sb.Append('/');
			sb.Append(HttpUtility.UrlEncode(Node.SourceId));

			if (!string.IsNullOrEmpty(Node.Partition))
			{
				sb.Append('/');
				sb.Append(HttpUtility.UrlEncode(Node.Partition));
			}

			sb.Append('/');
			sb.Append(HttpUtility.UrlEncode(Node.NodeId));
		}

		/// <summary>
		/// Appends semantic information about a node to a graph.
		/// </summary>
		/// <param name="Result">Graph being generated.</param>
		/// <param name="Node">NOde to append information about.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Caller, required.</param>
		public static async Task AppendNodeInformation(InMemorySemanticCube Result,
			INode Node, Language Language, RequestOrigin Caller)
		{
			if (Node is null)
				return;

			if (!await Node.CanViewAsync(Caller))
				return;

			UriNode NodeGraphUriNode = new UriNode(GetNodeUri(Node));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(DublinCore.Terms.Type),
				new UriNode(IoTConcentrator.Node)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.TypeName),
				new StringLiteral(await Node.GetTypeNameAsync(Language))));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.SourceId),
				new StringLiteral(Node.SourceId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.NodeId),
				new StringLiteral(Node.NodeId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.LogId),
				new StringLiteral(Node.LogId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.LocalId),
				new StringLiteral(Node.LocalId)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(RdfSchema.Label),
				new StringLiteral(Node.LocalId)));

			if (!string.IsNullOrEmpty(Node.Partition))
			{
				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.Partition),
					new StringLiteral(Node.Partition)));
			}

			if (!(Node.Parent is null))
			{
				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.ParentNode),
					new UriNode(GetNodeUri(Node.Parent))));
			}

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(DublinCore.Terms.Updated),
				new DateTimeLiteral(Node.LastChanged)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.HasChildSource),
				new BooleanLiteral(Node.HasChildren)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.HasCommands),
				new BooleanLiteral(Node.HasCommands)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.IsControllable),
				new BooleanLiteral(Node.IsControllable)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.IsReadable),
				new BooleanLiteral(Node.IsReadable)));

			Result.Add(new SemanticTriple(
				NodeGraphUriNode,
				new UriNode(IoTConcentrator.State),
				new CustomLiteral(Node.State.ToString(), IoTConcentrator.NodeState.AbsoluteUri)));

			//Node.GetDisplayableParametersAsync();
			//Node.GetMessagesAsync();

			if (Node.HasChildren)
			{
				foreach (INode ChildNode in await Node.ChildNodes)
				{
					Result.Add(new SemanticTriple(
						NodeGraphUriNode,
						new UriNode(IoTConcentrator.ChildNode),
						new UriNode(GetNodeUri(ChildNode))));
				}
			}

			if (Node.HasCommands)
			{
				BlankNode Commands = new BlankNode("n" + Guid.NewGuid().ToString());
				int CommandIndex = 0;

				Result.Add(new SemanticTriple(
					NodeGraphUriNode,
					new UriNode(IoTConcentrator.Commands),
					Commands));

				foreach (ICommand Command in await Node.Commands)
				{
					if (!await Command.CanExecuteAsync(Caller))
						continue;

					UriNode CommandUriNode = new UriNode(GetCommandUri(Node, Command));

					Result.Add(new SemanticTriple(
						Commands,
						new UriNode(Rdf.ListItem(++CommandIndex)),
						CommandUriNode));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(DublinCore.Terms.Type),
						new UriNode(IoTConcentrator.Command)));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(RdfSchema.Label),
						new StringLiteral(await Command.GetNameAsync(Language))));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(IoTConcentrator.CommandId),
						new StringLiteral(Command.CommandID)));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(IoTConcentrator.SortCategory),
						new StringLiteral(Command.SortCategory)));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(IoTConcentrator.SortKey),
						new StringLiteral(Command.SortKey)));

					Result.Add(new SemanticTriple(
						CommandUriNode,
						new UriNode(IoTConcentrator.CommandType),
						new CustomLiteral(Command.Type.ToString(), IoTConcentrator.CommandType.AbsoluteUri)));

					AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Success,
						await Command.GetSuccessStringAsync(Language));

					AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Failure,
						await Command.GetFailureStringAsync(Language));

					AddOptionalStringLiteral(Result, CommandUriNode, IoTConcentrator.Confirmation,
						await Command.GetConfirmationStringAsync(Language));
				}
			}
		}

		private static void AddOptionalStringLiteral(InMemorySemanticCube Result,
			ISemanticElement Subject, Uri PredicateUri, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				Result.Add(new SemanticTriple(
					Subject,
					new UriNode(PredicateUri),
					new StringLiteral(Value)));
			}
		}

		/// <summary>
		/// Gets the Graph URI for a node command.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command reference.</param>
		/// <returns>Graph URI for Node Command Graph.</returns>
		public static Uri GetCommandUri(INode Node, ICommand Command)
		{
			StringBuilder sb = new StringBuilder();
			GetCommandUri(Node, Command, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node command.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command reference.</param>
		/// <param name="sb">URI building built.</param>
		private static void GetCommandUri(INode Node, ICommand Command, StringBuilder sb)
		{
			GetNodeUri(Node, sb);
			sb.Append("/cmd/");
			sb.Append(HttpUtility.UrlEncode(Command.CommandID));
		}

		private static async Task<ISemanticCube> GetNodeActionGraph(InMemorySemanticCube Result,
			INode Node, Language Language, string Category, string Action)
		{
			switch (Category)
			{
				default:
					return null;
			}
		}

	}
}
