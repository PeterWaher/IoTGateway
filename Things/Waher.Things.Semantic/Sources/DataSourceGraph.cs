using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content.Semantic;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL;
using Waher.Things.Semantic.Sources.DynamicGraphs;

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
			if (!Gateway.IsDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				return Grade.NotAtAll;
			}

			string s = GraphUri.AbsolutePath;
			string[] Parts = s.Split('/');
			int c = Parts.Length;

			if (c < 1 || !string.IsNullOrEmpty(Parts[0]))
				return Grade.NotAtAll;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c == 1)
				return Grade.Ok;

			if (!Gateway.ConcentratorServer.TryGetDataSource(HttpUtility.UrlDecode(Parts[1]), out _))
				return Grade.NotAtAll;

			return Grade.Excellent;
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
			if (!Gateway.IsDomain(GraphUri.Host, true) ||
				!string.IsNullOrEmpty(GraphUri.Query) ||
				!string.IsNullOrEmpty(GraphUri.Fragment) ||
				Gateway.ConcentratorServer is null)
			{
				if (NullIfNotFound)
					return null;
				else
					throw new NotFoundException("Graph not found.");
			}

			IDynamicGraph DynamicGraph = await FindDynamicGraph(GraphUri.AbsolutePath, Caller);
			if (DynamicGraph is null)
			{
				if (NullIfNotFound)
					return null;
				else
					throw new NotFoundException("Graph not found.");
			}

			// TODO: Check access rights

			InMemorySemanticCube Result = new InMemorySemanticCube();
			Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

			await DynamicGraph.GenerateGraph(Result, Language, Caller);

			return Result;
		}

		/// <summary>
		/// Finds a dynamic graph, based on its relative path.
		/// </summary>
		/// <param name="Path">Resource path.</param>
		/// <param name="Caller">Information about entity making the request.</param>
		/// <returns>Dynamic graph, if found, null otherwise.</returns>
		public static async Task<IDynamicGraph> FindDynamicGraph(string Path, RequestOrigin Caller)
		{
			string[] Parts = Path.Split('/');
			int c = Parts.Length;

			if (c < 1 || !string.IsNullOrEmpty(Parts[0]))
				return null;

			if (string.IsNullOrEmpty(Parts[c - 1]))
				c--;

			if (c == 1)
				return new MachineGraph();

			string SourceID = HttpUtility.UrlDecode(Parts[1]);
			if (!Gateway.ConcentratorServer.TryGetDataSource(SourceID, out IDataSource Source))
				return null;

			if (!await Source.CanViewAsync(Caller))
				return null;

			if (c == 2) // DOMAIN/Source
				return new SourceGraph(Source);

			switch (c)
			{
				case 3: // /DOMAIN/Source/NodeID
					string NodeID = HttpUtility.UrlDecode(Parts[2]);
					INode NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					if (!await NodeObj.CanViewAsync(Caller))
						return null;

					return new NodeGraph(NodeObj);

				case 4: // /DOMAIN/Source/Partition/NodeID
					string Partition = HttpUtility.UrlDecode(Parts[2]);
					NodeID = HttpUtility.UrlDecode(Parts[3]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					if (!await NodeObj.CanViewAsync(Caller))
						return null;

					return new NodeGraph(NodeObj);

				case 5: // /DOMAIN/Source/NodeID/Category/Action
					NodeID = HttpUtility.UrlDecode(Parts[2]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID));
					if (NodeObj is null)
						return null;

					if (!await NodeObj.CanViewAsync(Caller))
						return null;

					return HttpUtility.UrlDecode(Parts[3]) switch
					{
						"read" => new NodeReadActionGraph(NodeObj, HttpUtility.UrlDecode(Parts[4])),
						// TODO: cmd, edit, control
						_ => null,
					};
				case 6: // /DOMAIN/Source/Partition/NodeID/Category/Action
					Partition = HttpUtility.UrlDecode(Parts[2]);
					NodeID = HttpUtility.UrlDecode(Parts[3]);
					NodeObj = await Source.GetNodeAsync(new ThingReference(NodeID, SourceID, Partition));
					if (NodeObj is null)
						return null;

					if (!await NodeObj.CanViewAsync(Caller))
						return null;

					return HttpUtility.UrlDecode(Parts[4]) switch
					{
						"read" => new NodeReadActionGraph(NodeObj, HttpUtility.UrlDecode(Parts[5])),
						// TODO: cmd, edit, control
						_ => null,
					};
				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the Graph URI for a data source.
		/// </summary>
		/// <param name="SourceID">Source ID</param>
		/// <returns>Graph URI for the Source Graph.</returns>
		public static Uri GetSourceUri(string SourceID)
		{
			return new Uri(Gateway.GetUrl("/" + HttpUtility.UrlEncode(SourceID)));
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
		public static void GetNodeUri(INode Node, StringBuilder sb)
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
		public static void GetCommandUri(INode Node, ICommand Command, StringBuilder sb)
		{
			GetNodeUri(Node, sb);
			sb.Append("/cmd/");
			sb.Append(HttpUtility.UrlEncode(Command.CommandID));
		}

		/// <summary>
		/// Gets the Graph URI for a node operation.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Category">Operation category.</param>
		/// <param name="Action">Operation action</param>
		public static Uri GetOperationUri(INode Node, string Category, string Action)
		{
			StringBuilder sb = new StringBuilder();
			GetOperationUri(Node, Category, Action, sb);
			return new Uri(Gateway.GetUrl(sb.ToString()));
		}

		/// <summary>
		/// Gets the Graph URI for a node operation.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <param name="Category">Operation category.</param>
		/// <param name="Action">Operation action</param>
		/// <param name="sb">URI building built.</param>
		public static void GetOperationUri(INode Node, string Category, string Action, StringBuilder sb)
		{
			GetNodeUri(Node, sb);
			sb.Append('/');
			sb.Append(Category);
			sb.Append('/');
			sb.Append(Action);
		}

	}
}
