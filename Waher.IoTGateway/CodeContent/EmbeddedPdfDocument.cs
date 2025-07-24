using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Handles an embedded PDF document on a Markdown page.
	/// </summary>
	public class EmbeddedPdfDocument : ICodeContent, ICodeContentHtmlRenderer
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
	}
}
