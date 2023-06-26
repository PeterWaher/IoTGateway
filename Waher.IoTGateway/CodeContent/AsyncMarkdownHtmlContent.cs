using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html.Elements;
using Waher.Content.Markdown;
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
		/// <returns>ID to report back, when content is completed.</returns>
		public Task<string> GenerateStub(MarkdownOutputType Type, StringBuilder Output, string Title)
		{
			string Id = Hashes.BinaryToString(Gateway.NextBytes(32));

			if (string.IsNullOrEmpty(Title))
				Title = "&#8987;";
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

			return Task.FromResult(Id);
		}

		/// <summary>
		/// Method called when asynchronous result has been generated in a Markdown document.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Id">ID of generated content.</param>
		/// <param name="Result">Generated content.</param>
		public Task ReportResult(MarkdownOutputType Type, string Id, string Result)
		{
			return ClientEvents.ReportAsynchronousResult(Id, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(Result), false);
		}

	}
}
