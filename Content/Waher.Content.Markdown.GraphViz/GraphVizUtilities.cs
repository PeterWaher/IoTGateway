using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
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
			return GenerateGraph(null, Graph, Title);
		}

		/// <summary>
		/// Generates a GraphViz dot graph from a semantic graph.
		/// </summary>
		/// <param name="ResultSet">SPARQL Result Set for shortening URLs and Blank Nodes</param>
		/// <param name="Graph">Semantic graph.</param>
		/// <param name="Title">Title of graph.</param>
		/// <returns>GraphViz dot graph.</returns>
		public static string GenerateGraph(SparqlResultSet ResultSet, ISemanticModel Graph, string Title)
		{
			if (ResultSet is null)
				ResultSet = new SparqlResultSet(false); // Object only used for shortening URIs and Blank Nodes.

			StringBuilder sb = new StringBuilder();
			Dictionary<string, string> NodeNames = new Dictionary<string, string>();

			sb.AppendLine("digraph G {");
			sb.AppendLine("\trankdir=LR");
			sb.AppendLine("\tbgcolor=\"transparent\"");
			sb.AppendLine("\tnode [style=filled,fillcolor=white]");

			if (!string.IsNullOrEmpty(Title))
			{
				sb.Append("\tlabel=\"");
				sb.Append(JSON.Encode(Title));
				sb.AppendLine("\"");
				sb.AppendLine("\tlabelloc=\"t\"");
				sb.AppendLine("\tfontsize=20");
			}

			int LiteralIndex = 0;

			foreach (ISemanticTriple Triple in Graph)
			{
				AddShortName(ResultSet, Triple.Subject, NodeNames, sb, ref LiteralIndex);
				AddShortName(ResultSet, Triple.Object, NodeNames, sb, ref LiteralIndex);
			}

			LiteralIndex = 0;

			foreach (ISemanticTriple Triple in Graph)
			{
				sb.Append("\t");
				sb.Append(GetNodeName(ResultSet, Triple.Subject, NodeNames, false, ref LiteralIndex));
				sb.Append(" -> ");
				sb.Append(GetNodeName(ResultSet, Triple.Object, NodeNames, false, ref LiteralIndex));
				sb.Append(" [label=\"");
				sb.Append(JSON.Encode(GetNodeName(ResultSet, Triple.Predicate, null, true, ref LiteralIndex)));
				sb.AppendLine("\"]");
			}

			sb.AppendLine("}");

			return sb.ToString();
		}

		private static string GetNodeName(SparqlResultSet Result, ISemanticElement Element,
			Dictionary<string, string> NodeNames, bool IgnoreLiteral, ref int LiteralIndex)
		{
			string Name;

			if (Element is UriNode UriNode)
				Name = Result.GetShortUri(UriNode);
			else if (Element is BlankNode BlankNode)
				Name = Result.GetShortBlankNodeLabel(BlankNode);
			else if (Element is SemanticLiteral)
			{
				if (IgnoreLiteral)
					return "\"" + Element.ToString().Replace("\"", "\\\"") + "\"";
				else
					return "L" + (++LiteralIndex).ToString();
			}
			else
				Name = Element.ToString();

			if (NodeNames is null)
				return Name;
			else
				return NodeNames[Name];
		}

		private static void AddShortName(SparqlResultSet Result, ISemanticElement Element,
			Dictionary<string, string> NodeNames, StringBuilder sb, ref int LiteralIndex)
		{
			string Name;
			string NodeName = null;
			string AdditionalStyle = null;

			if (Element is UriNode UriNode)
			{
				Name = Result.GetShortUri(UriNode);
				AdditionalStyle = ",fillcolor=azure";
			}
			else if (Element is BlankNode BlankNode)
			{
				Name = Result.GetShortBlankNodeLabel(BlankNode);
				AdditionalStyle = ",fillcolor=lightgray";
			}
			else if (Element is SemanticLiteral SemanticLiteral)
			{
				string Type = SemanticLiteral.StringType;
				string ShortType = Result.GetShortUri(SemanticLiteral.StringType);

				NodeName = "L" + (++LiteralIndex).ToString();
				AdditionalStyle = ",fillcolor=honeydew,shape=box,style=\"rounded,filled\",margin=\"0.2,0\",height=0.35";

				if (ShortType == Type)
					Name = Element.ToString();
				else if (SemanticLiteral is CustomLiteral CustomLiteral)
				{
					Name = CustomLiteral.ToString(CustomLiteral.StringValue,
						CustomLiteral.Language, ShortType);
				}
				else
				{
					Name = CustomLiteral.ToString(SemanticLiteral.StringValue,
						null, ShortType);
				}
			}
			else
				Name = Element.ToString();

			if (string.IsNullOrEmpty(NodeName))
			{
				NodeName = "N" + (NodeNames.Count + 1).ToString();
				if (NodeNames.ContainsKey(Name))
					return;
				
				NodeNames[Name] = NodeName;
			}

			sb.Append("\t");
			sb.Append(NodeName);
			sb.Append(" [label=\"");
			sb.Append(JSON.Encode(Name));
			sb.Append('"');

			if (!string.IsNullOrEmpty(AdditionalStyle))
				sb.Append(AdditionalStyle);

			sb.AppendLine("]");
		}

		#endregion
	}
}
