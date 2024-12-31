using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Functions;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Pushes asynchronously generated HTML content to clients
	/// </summary>
	public class AsyncMarkdownHtmlContent : IMarkdownAsynchronousOutput
	{
		/// <summary>
		/// Pushes asynchronously generated HTML content to clients
		/// </summary>
		public AsyncMarkdownHtmlContent()
		{
		}

		/// <summary>
		/// How well specific content type is supported.
		/// </summary>
		/// <param name="OutputType">Output type.</param>
		/// <returns>How well the output type is supported.</returns>
		public Grade Supports(MarkdownOutputType OutputType)
		{
			return OutputType == MarkdownOutputType.Html ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Generates a stub in the output, that will be filled with the asynchronously generated
		/// content, once it is reported.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Output">Output being generated.</param>
		/// <param name="Title">Title of content.</param>
		/// <param name="Document">Markdown Document being rendered.</param>
		/// <returns>ID to report back, when content is completed.</returns>
		public async Task<string> GenerateStub(MarkdownOutputType Type, StringBuilder Output, string Title, MarkdownDocument Document)
		{
			string Id = Hashes.BinaryToString(Gateway.NextBytes(32));

			if (string.IsNullOrEmpty(Title))
				Title = await MarkdownToHtml.ToHtml(":hourglass_flowing_sand:", Document.Settings.Variables);
			else
				Title = XML.HtmlValueEncode(Title);

			Output.Append("<div id=\"id");
			Output.Append(Id);
			Output.Append("\">");
			Output.Append(Title);
			Output.AppendLine("</div>");
			Output.Append("<script type=\"text/javascript\">LoadContent(\"");
			Output.Append(Id);
			Output.AppendLine("\");</script>");

			return Id;
		}

		/// <summary>
		/// Method called when asynchronous result has been generated in a Markdown document.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Id">ID of generated content.</param>
		/// <param name="Result">Generated content.</param>
		public Task ReportResult(MarkdownOutputType Type, string Id, string Result)
		{
			return this.ReportResult(Type, Id, Result, false);
		}

		/// <summary>
		/// Method called when asynchronous result has been generated in a Markdown document.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Id">ID of generated content.</param>
		/// <param name="Result">Generated content.</param>
		/// <param name="More">If more information will be sent in another call.</param>
		public Task ReportResult(MarkdownOutputType Type, string Id, string Result, bool More)
		{
			return ClientEvents.ReportAsynchronousResult(Id, "text/html; charset=utf-8", 
				true, System.Text.Encoding.UTF8.GetBytes(Result), More);
		}
	}
}
