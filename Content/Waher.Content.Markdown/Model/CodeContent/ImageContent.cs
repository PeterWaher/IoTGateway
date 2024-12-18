using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
		/// Checks how well the handler supports code content of a given type.
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
		/// If script is evaluated for this type of code block.
		/// </summary>
		public bool EvaluatesScript => false;

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
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

				GenerateMarkdown(Output, Bin, ContentType, Title);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Generates Markdown embedding an encoded image.
		/// </summary>
		/// <param name="Output">Markdown output.</param>
		/// <param name="Bin">Binary encoding of image.</param>
		/// <param name="ContentType">Content-Type of image.</param>
		/// <param name="Title">Optional title.</param>
		public static void GenerateMarkdown(StringBuilder Output, byte[] Bin, string ContentType, string Title)
		{
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
		}

		/// <summary>
		/// Generates a data URL of an encoded image.
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
		/// Generates a data URL of an encoded image.
		/// </summary>
		/// <param name="Binary">Binary representation of image.</param>
		/// <param name="ContentType">Content-Type of image.</param>
		/// <returns>Data URL.</returns>
		public static string GenerateUrl(byte[] Binary, string ContentType)
		{
			StringBuilder Output = new StringBuilder();

			Output.Append("data:");
			Output.Append(ContentType);
			Output.Append(";base64,");
			Output.Append(Convert.ToBase64String(Binary));

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
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		public async Task<PixelInformation> GenerateImage(string[] Rows, string Language, MarkdownDocument Document)
		{
			ContentResponse Obj = await InternetContent.DecodeAsync(Language, Convert.FromBase64String(GetImageBase64(Rows)), null);
			if (Obj.HasError || !(Obj.Decoded is SKImage Image))
				throw new Exception("Unable to decode raw data as an image.");

			return PixelInformation.FromImage(Image);
		}
	}
}
