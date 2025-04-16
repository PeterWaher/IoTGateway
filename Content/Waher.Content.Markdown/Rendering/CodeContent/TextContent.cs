using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Json;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Runtime.IO;

namespace Waher.Content.Markdown.Rendering.CodeContent
{
	/// <summary>
	/// Base64-encoded text content.
	/// </summary>
	public class TextContent : Model.CodeContent.TextContent, ICodeContentHtmlRenderer, ICodeContentTextRenderer, ICodeContentMarkdownRenderer
	{
		/// <summary>
		/// Base64-encoded text content.
		/// </summary>
		public TextContent()
		{
		}

		/// <summary>
		/// Decodes a BASE64-encoded text string.
		/// </summary>
		/// <param name="Rows">BASE64-encoded string</param>
		/// <returns>Decoded string.</returns>
		public static string DecodeBase64EncodedText(string[] Rows)
		{
			return DecodeBase64EncodedText(Rows, Encoding.UTF8);
		}

		/// <summary>
		/// Decodes a BASE64-encoded text string.
		/// </summary>
		/// <param name="Rows">BASE64-encoded string</param>
		/// <param name="ContentType">Content-Type</param>
		/// <returns>Decoded string.</returns>
		public static string DecodeBase64EncodedText(string[] Rows, ref string ContentType)
		{
			Encoding Encoding = Encoding.UTF8;
			int i = ContentType.IndexOf(':');

			if (i > 0)
				ContentType = ContentType.Substring(0, i).TrimEnd();

			i = ContentType.IndexOf(';');

			if (i > 0)
			{
				KeyValuePair<string, string>[] ContentTypeFields = CommonTypes.ParseFieldValues(
					ContentType.Substring(i + 1).TrimStart());

				ContentType = ContentType.Substring(0, i).TrimEnd();

				foreach (KeyValuePair<string, string> P in ContentTypeFields)
				{
					if (string.Compare(P.Key, "charset", true) == 0)
					{
						Encoding = Encoding.GetEncoding(P.Value);
						break;
					}
				}
			}

			return DecodeBase64EncodedText(Rows, Encoding);
		}

		/// <summary>
		/// Decodes a BASE64-encoded text string.
		/// </summary>
		/// <param name="Rows">BASE64-encoded string</param>
		/// <param name="Encoding">Default text encoding. (Default=UTF-8)</param>
		/// <returns>Decoded string.</returns>
		public static string DecodeBase64EncodedText(string[] Rows, Encoding Encoding)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string Row in Rows)
				sb.Append(Row);

			return Strings.GetString(Convert.FromBase64String(sb.ToString()), Encoding);
		}

		/// <summary>
		/// Makes text pretty.
		/// </summary>
		/// <param name="Text">Text</param>
		/// <param name="ContentType">Content-Type of text.</param>
		/// <returns>Pretty text.</returns>
		public static string MakePretty(string Text, string ContentType)
		{
			return MakePretty(Text, ContentType, out _);
		}

		/// <summary>
		/// Makes text pretty.
		/// </summary>
		/// <param name="Text">Text</param>
		/// <param name="ContentType">Content-Type of text.</param>
		/// <param name="ClassName">Associated class name.</param>
		/// <returns>Pretty text.</returns>
		public static string MakePretty(string Text, string ContentType, out string ClassName)
		{
			if (Array.IndexOf(XmlCodec.XmlContentTypes, ContentType) >= 0)
			{
				ClassName = "xml";
				return XML.PrettyXml(Text);
			}
			else if (Array.IndexOf(JsonCodec.JsonContentTypes, ContentType) >= 0)
			{
				ClassName = "json";
				return JSON.PrettyJson(Text);
			}
			else
			{
				ClassName = null;
				return Text;
			}
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
			string Text = DecodeBase64EncodedText(Rows, ref Language);
			Text = MakePretty(Text, Language, out string ClassName);

			Output.Append("<pre><code class=\"");

			if (string.IsNullOrEmpty(ClassName))
				Output.Append("nohighlight");
			else
				Output.Append(ClassName);

			Output.Append("\">");
			Output.Append(Text);
			Output.AppendLine("</code></pre>");

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
			string Text = DecodeBase64EncodedText(Rows, ref Language);
			Text = MakePretty(Text, Language);

			Renderer.Output.AppendLine(Text);
			return Task.FromResult(true);
		}

		/// <summary>
		/// Generates Markdown for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public Task<bool> RenderMarkdown(MarkdownRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;
			string Text = DecodeBase64EncodedText(Rows, ref Language);
			Text = MakePretty(Text, Language, out string ClassName);

			Output.Append("```");
			Output.AppendLine(ClassName ?? string.Empty);
			Output.AppendLine(Text);
			Output.AppendLine("```");
			Output.AppendLine();

			return Task.FromResult(true);
		}
	}
}
