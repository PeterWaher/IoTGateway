using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Xamarin.CodeContent
{
	/// <summary>
	/// Base64-encoded text content.
	/// </summary>
	public class TextContent : Model.CodeContent.TextContent, ICodeContentXamarinFormsXamlRenderer
	{
		/// <summary>
		/// Base64-encoded text content.
		/// </summary>
		public TextContent()
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string Text = Rendering.CodeContent.TextContent.DecodeBase64EncodedText(Rows, ref Language);
			Text = Rendering.CodeContent.TextContent.MakePretty(Text, Language);

			Rows = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

			XmlWriter XmlOutput = Renderer.XmlOutput;

			Renderer.RenderContentView();
			XmlOutput.WriteStartElement("StackLayout");
			XmlOutput.WriteAttributeString("Orientation", "Vertical");

			foreach (string Row in Rows)
			{
				XmlOutput.WriteStartElement("Label");
				XmlOutput.WriteAttributeString("LineBreakMode", "NoWrap");
				Renderer.RenderLabelAlignment();
				XmlOutput.WriteAttributeString("FontFamily", "Courier New");
				XmlOutput.WriteAttributeString("Text", Row);
				XmlOutput.WriteEndElement();
			}

			XmlOutput.WriteEndElement();
			XmlOutput.WriteEndElement();

			return Task.FromResult(true);
		}

	}
}
