using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Rendering.CodeContent
{
	/// <summary>
	/// Base64-encoded image content.
	/// </summary>
	public class ImageContent : Model.CodeContent.ImageContent, ICodeContentHtmlRenderer, ICodeContentTextRenderer
	{
		/// <summary>
		/// Base64-encoded image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates HTML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderHtml(HtmlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;

			Output.Append("<figure>");
			Output.Append("<img class=\"aloneUnsized\" src=\"");
			Output.Append(GenerateUrl(Language, Rows, out string _, out string Title));
			Output.Append('"');

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append(" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Title));
				Output.Append(" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Title));
				Output.Append("\"/><figcaption>");
				Output.Append(XML.HtmlValueEncode(Title));
				Output.AppendLine("</figcaption></figure>");
			}
			else
				Output.AppendLine(" alt=\"Image\"/></figure>");

			return Task.FromResult(true);
		}

		/// <summary>
		/// Generates plain text for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderText(TextRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;
			int i = Language.IndexOf(':');

			if (i < 0)
				Output.AppendLine(Language);
			else
				Output.AppendLine(Language.Substring(i + 1));

			Output.AppendLine();

			return Task.FromResult(true);
		}
	}
}
