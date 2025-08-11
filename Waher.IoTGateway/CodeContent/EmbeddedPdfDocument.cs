using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Handles an embedded PDF document on a Markdown page.
	/// </summary>
	public class EmbeddedPdfDocument : MultimediaContent, ICodeContent, ICodeContentHtmlRenderer,
		IMultimediaHtmlRenderer
	{
		private MarkdownDocument document;

		/// <summary>
		/// Handles an embedded PDF document on a Markdown page.
		/// </summary>
		public EmbeddedPdfDocument()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			int i = Language.IndexOf(':');
			if (i > 0)
				Language = Language[..i].TrimEnd();

			if (string.Compare(Language, "application/pdf", true) == 0)
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (Item.ContentType.StartsWith("application/pdf") || Item.Url.EndsWith(".pdf"))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			return true;
		}

		/// <summary>
		/// If script is evaluated for this type of code block.
		/// </summary>
		public bool EvaluatesScript => false;

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
			this.document = Document;
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
			int i = Language.IndexOf(':');

			StringBuilder Output = Renderer.Output;

			Output.Append("<embed type=\"application/pdf\"");
			Output.Append(" style=\"margin-bottom:1em;width:100%;height:75vh;\"");

			if (i > 0)
			{
				Output.Append(" title=\"");
				Output.Append(XML.HtmlValueEncode(Language[(i + 1)..].Trim()));
				Output.Append('"');
			}

			Output.Append(" src=\"data:application/pdf;base64,");

			foreach (string Row in Rows)
				Output.Append(Row.Trim());

			Output.AppendLine("\"/>");

			return Task.FromResult(true);
		}
		/// <summary>
		/// Generates HTML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderHtml(HtmlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, bool AloneInParagraph,
			MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;

			Output.Append("<embed type=\"application/pdf\"");
			Output.Append(" style=\"margin-bottom:1em;width:100%;height:75vh;\"");
			Output.Append(" src=\"");

			foreach (MultimediaItem Item in Items)
			{
				Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Item.Url, null)));
				break;
			}

			Output.AppendLine("\">");

			if (AloneInParagraph)
				Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Reports a resource for preloading.
		/// </summary>
		/// <param name="Progress">Progress reporting interface.</param>
		/// <param name="Items">Multi-media items.</param>
		public Task Preload(ICodecProgress Progress, MultimediaItem[] Items)
		{
			return Task.CompletedTask;
		}

	}
}
