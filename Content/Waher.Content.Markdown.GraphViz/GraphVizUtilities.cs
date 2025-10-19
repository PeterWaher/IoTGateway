using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script;

namespace Waher.Content.Markdown.GraphViz
{
	/// <summary>
	/// GraphViz conversion utilities.
	/// </summary>
	public static class GraphVizUtilities
	{
		#region Converting a GraphViz diagram to an image

		/// <summary>
		/// Converts a GraphViz dot graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> DotToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("dot", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz neato graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> NeatoToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("neato", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz fdp graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> FdpToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("fdp", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz sfdp graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> SfdpToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("sfdp", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz twopi graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> TwoPiToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("twopi", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz circo graph description to an image.
		/// </summary>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> CircoToImage(string GraphText, ResultType ResultType)
		{
			return GraphVizToImage("circo", GraphText, ResultType);
		}

		/// <summary>
		/// Converts a GraphViz graph description to an image.
		/// </summary>
		/// <param name="Language">Graph language.</param>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <returns>Binary image.</returns>
		public static Task<byte[]> GraphVizToImage(string Language, string GraphText,
			ResultType ResultType)
		{
			return GraphVizToImage(Language, GraphText, ResultType, new Variables());
		}

		/// <summary>
		/// Converts a GraphViz graph description to an image.
		/// </summary>
		/// <param name="Language">Graph language.</param>
		/// <param name="GraphText">Graph description.</param>
		/// <param name="ResultType">Type of image to return.</param>
		/// <param name="Variables">Variables for color definitions.</param>
		/// <returns>Binary image.</returns>
		public static async Task<byte[]> GraphVizToImage(string Language, string GraphText,
			ResultType ResultType, Variables Variables)
		{
			GraphInfo Graph = await GraphViz.GetFileName(Language, GraphText, ResultType, true, Variables);
			return await Runtime.IO.Files.ReadAllBytesAsync(Graph.FileName);
		}

		#endregion

		#region Converting semantic graphs to a GraphViz diagram

		/// <summary>
		/// Generates a GraphViz dot graph from a semantic graph.
		/// </summary>
		/// <param name="Graph">Semantic graph.</param>
		/// <param name="Title">Title of graph.</param>
		/// <returns>GraphViz dot graph.</returns>
		public static string GenerateGraph(ISemanticModel Graph, string Title)
		{
			StringBuilder sb = new StringBuilder();
			Dictionary<string, string> NodeNames = new Dictionary<string, string>();

			sb.AppendLine("digraph G {");
			sb.AppendLine("\trankdir=LR");
			sb.AppendLine("\tbgcolor=\"transparent\"");
			sb.AppendLine("\tnode [style=filled,fillcolor=white]");
			sb.Append("\tlabel=\"");
			sb.Append(JSON.Encode(Title).Replace("\\", "\\\\").Replace("\"", "\\\""));
			sb.AppendLine("\"");
			sb.AppendLine("\tlabelloc=\"t\"");
			sb.AppendLine("\tfontsize=20");

			foreach (ISemanticTriple Triple in Graph)
			{
				AddName(Triple.Subject.ToString(), NodeNames, sb);
				AddName(Triple.Object.ToString(), NodeNames, sb);
			}

			foreach (ISemanticTriple Triple in Graph)
			{
				sb.Append("\t");
				sb.Append(NodeNames[Triple.Subject.ToString()]);
				sb.Append(" -> ");
				sb.Append(NodeNames[Triple.Object.ToString()]);
				sb.Append(" [label=\"");
				sb.Append(JSON.Encode(Triple.Predicate.ToString()).Replace("\\", "\\\\").Replace("\"", "\\\""));
				sb.AppendLine("\"]");
			}

			sb.AppendLine("}");

			return sb.ToString();
		}

		private static void AddName(string Node, Dictionary<string, string> NodeNames,
			StringBuilder sb)
		{
			if (!NodeNames.ContainsKey(Node))
			{
				string NodeName = "N" + (NodeNames.Count + 1).ToString();
				NodeNames[Node] = NodeName;
				sb.Append("\t");
				sb.Append(NodeName);
				sb.Append(" [label=\"");
				sb.Append(JSON.Encode(Node).Replace("\\", "\\\\").Replace("\"", "\\\""));
				sb.AppendLine("\"]");
			}
		}

		#endregion
	}
}
