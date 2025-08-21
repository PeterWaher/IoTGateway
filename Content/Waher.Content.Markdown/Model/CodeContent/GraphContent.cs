using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Model.CodeContent
{
	/// <summary>
	/// Script graph content.
	/// </summary>
	public class GraphContent : IImageCodeContent
	{
		/// <summary>
		/// Script graph content.
		/// </summary>
		public GraphContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports code content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			return string.Compare(Language, "graph", true) == 0 ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// If script is evaluated for this type of code block.
		/// </summary>
		public bool EvaluatesScript => false;	// Embedded script is tested to use only safe script, such as constants and constructs with constants.

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
		}

		/// <summary>
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		public async Task<PixelInformation> GenerateImage(string[] Rows, string Language, MarkdownDocument Document)
		{
			Graph G = await GetGraph(Rows);
			return G.CreatePixels();
		}

		/// <summary>
		/// Gets a graph object from its XML Code Block representation.
		/// </summary>
		/// <param name="Rows">Rows</param>
		/// <returns>Graph object</returns>
		public static async Task<Graph> GetGraph(string[] Rows)
		{
			XmlDocument Xml = new XmlDocument();
			Xml.LoadXml(MarkdownDocument.AppendRows(Rows));

			return await Graph.Import(Xml.DocumentElement);
		}
	}
}
