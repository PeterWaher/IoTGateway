using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Contracts.CodeContent
{
	/// <summary>
	/// Base64-encoded text content.
	/// </summary>
	public class TextContent : Model.CodeContent.TextContent, ICodeContentContractsRenderer
	{
		/// <summary>
		/// Base64-encoded text content.
		/// </summary>
		public TextContent()
		{
		}

		/// <summary>
		/// Generates smart contract XML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderContractXml(ContractsRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string Text = Rendering.CodeContent.TextContent.DecodeBase64EncodedText(Rows, ref Language);
			Text = Rendering.CodeContent.TextContent.MakePretty(Text, Language);

			Rows = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

			XmlWriter XmlOutput = Renderer.XmlOutput;
			XmlOutput.WriteStartElement("paragraph");

			bool First = true;

			foreach (string Row in Rows)
			{
				if (First)
					First = false;
				else
					XmlOutput.WriteElementString("lineBreak", string.Empty);

				XmlOutput.WriteElementString("text", Row);
			}

			XmlOutput.WriteEndElement();

			return Task.FromResult(true);
		}

	}
}
