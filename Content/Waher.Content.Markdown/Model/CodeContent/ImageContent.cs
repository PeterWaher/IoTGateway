using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Emoji;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Model.CodeContent
{
	/// <summary>
	/// Base64-encoded image content.
	/// </summary>
	public class ImageContent : IImageCodeContent
	{
		/// <summary>
		/// Base64-encoded image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			if (Language.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
				return InternetContent.TryGetFileExtension(Language, out _) ? Grade.Ok : Grade.Barely;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
		}

		/// <summary>
		/// If (transportable) Markdown is handled.
		/// </summary>
		public bool HandlesMarkdown => false;

		/// <summary>
		/// If HTML is handled.
		/// </summary>
		public bool HandlesHTML => true;

		/// <summary>
		/// If Plain Text is handled.
		/// </summary>
		public bool HandlesPlainText => true;

		/// <summary>
		/// If XAML is handled.
		/// </summary>
		public bool HandlesXAML => true;

		/// <summary>
		/// If LaTeX is handled.
		/// </summary>
		public bool HandlesLaTeX => true;

		/// <summary>
		/// If smart-contract XML is handled.
		/// </summary>
		public bool HandlesSmartContract => true;

		/// <summary>
		/// Generates (transportanle) Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public Task<bool> GenerateMarkdown(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Generates Markdown embedding an image available in a file.
		/// </summary>
		/// <param name="Output">Markdown output.</param>
		/// <param name="FileName">Image file name.</param>
		/// <param name="Title">Optional title.</param>
		/// <returns>If Markdown could be generated.</returns>
		public static async Task<bool> GenerateMarkdownFromFile(StringBuilder Output, string FileName, string Title)
		{
			if (!InternetContent.TryGetContentType(Path.GetExtension(FileName), out string ContentType))
				return false;

			try
			{
				byte[] Bin = await Resources.ReadAllBytesAsync(FileName);

				Output.Append("```");
				Output.Append(ContentType);

				if (!string.IsNullOrEmpty(Title))
				{
					Output.Append(':');
					Output.Append(Title);
				}

				Output.AppendLine();
				Output.AppendLine(Convert.ToBase64String(Bin));
				Output.AppendLine("```");
				Output.AppendLine();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public Task<bool> GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
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
		/// Generates a data URL of the encoded image.
		/// </summary>
		/// <param name="Language">Language</param>
		/// <param name="Rows">Rows</param>
		/// <param name="ContentType">Content-Type of image.</param>
		/// <param name="Title">Optional associated title.</param>
		/// <returns>Data URL.</returns>
		public static string GenerateUrl(string Language, string[] Rows, out string ContentType, out string Title)
		{
			int i = Language.IndexOf(':');
			ContentType = i < 0 ? Language : Language.Substring(0, i);
			Title = i < 0 ? string.Empty : Language.Substring(i + 1);

			StringBuilder Output = new StringBuilder();

			Output.Append("data:");
			Output.Append(ContentType);
			Output.Append(";base64,");
			Output.Append(GetImageBase64(Rows));

			return Output.ToString();
		}

		/// <summary>
		/// Gets the binary image from the encoded rows.
		/// </summary>
		/// <param name="Rows">Rows</param>
		/// <returns>Base64-encoded image.</returns>
		public static string GetImageBase64(string[] Rows)
		{
			return MarkdownDocument.AppendRows(Rows, true);
		}

		/// <summary>
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public Task<bool> GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			int i = Language.IndexOf(':');
			if (i < 0)
				Output.AppendLine(Language);
			else
				Output.AppendLine(Language.Substring(i + 1));

			Output.AppendLine();

			return Task.FromResult(true);
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			await Multimedia.ImageContent.OutputWpf(Output, new ImageSource()
			{
				Url = GenerateUrl(Language, Rows, out _, out string Title)
			}, Title);

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			await Multimedia.ImageContent.OutputXamarinForms(Output, new ImageSource()
			{
				Url = GenerateUrl(Language, Rows, out _, out _)
			});

			return true;
		}

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateLaTeX(StringBuilder Output, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			byte[] Bin = Convert.FromBase64String(GetImageBase64(Rows));
			string FileName = await Multimedia.ImageContent.GetTemporaryFile(Bin);

			Output.AppendLine("\\begin{figure}[h]");
			Output.AppendLine("\\centering");

			Output.Append("\\fbox{\\includegraphics{");
			Output.Append(FileName.Replace('\\', '/'));
			Output.AppendLine("}}");

			Output.AppendLine("\\end{figure}");
			Output.AppendLine();

			return true;
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
			object Obj = await InternetContent.DecodeAsync(Language, Convert.FromBase64String(GetImageBase64(Rows)), null);
			if (!(Obj is SKImage Image))
				throw new Exception("Unable to decode raw data as an image.");

			return PixelInformation.FromImage(Image);
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State,
			string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			try
			{
				int i = Language.IndexOf(':');
				string ContentType = i < 0 ? Language : Language.Substring(0, i);
				string Title = i < 0 ? string.Empty : Language.Substring(i + 1);
				string Base64 = GetImageBase64(Rows);
				byte[] Bin = Convert.FromBase64String(Base64);

				object Decoded = await InternetContent.DecodeAsync(ContentType, Bin, null);

				if (!(Decoded is SKImage Image))
					return false;

				Output.WriteStartElement("imageStandalone");
				Output.WriteAttributeString("contentType", ContentType);
				Output.WriteAttributeString("width", Image.Width.ToString());
				Output.WriteAttributeString("height", Image.Height.ToString());

				Output.WriteStartElement("binary");
				Output.WriteValue(Base64);
				Output.WriteEndElement();

				Output.WriteStartElement("caption");
				if (string.IsNullOrEmpty(Title))
					Output.WriteElementString("text", "Image");
				else
					Output.WriteElementString("text", Title);

				Output.WriteEndElement();
				Output.WriteEndElement();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
