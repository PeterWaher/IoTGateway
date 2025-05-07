using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Wpf.CodeContent
{
	/// <summary>
	/// Base64-encoded text content.
	/// </summary>
	public class TextContent : Model.CodeContent.TextContent, ICodeContentWpfXamlRenderer
	{
		/// <summary>
		/// Base64-encoded text content.
		/// </summary>
		public TextContent()
		{
		}

		/// <summary>
		/// Generates WPF XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderWpfXaml(WpfXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string Text = Rendering.CodeContent.TextContent.DecodeBase64EncodedText(Rows, ref Language);
			Text = Rendering.CodeContent.TextContent.MakePretty(Text, Language);

			Rows = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

			XmlWriter XmlOutput = Renderer.XmlOutput;
			bool First = true;

			XmlOutput.WriteStartElement("TextBlock");
			XmlOutput.WriteAttributeString("xml", "space", null, "preserve");
			XmlOutput.WriteAttributeString("TextWrapping", "NoWrap");
			XmlOutput.WriteAttributeString("Margin", Renderer.XamlSettings.ParagraphMargins);
			XmlOutput.WriteAttributeString("FontFamily", "Courier New");
			
			if (Renderer.Alignment != TextAlignment.Left)
				XmlOutput.WriteAttributeString("TextAlignment", Renderer.Alignment.ToString());

			foreach (string Row in Rows)
			{
				if (First)
					First = false;
				else
					XmlOutput.WriteElementString("LineBreak", string.Empty);

				XmlOutput.WriteValue(Row);
			}

			XmlOutput.WriteEndElement();

			return Task.FromResult(true);
		}

	}
}
