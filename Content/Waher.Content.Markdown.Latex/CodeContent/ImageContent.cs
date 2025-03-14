using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html.Elements;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Latex.CodeContent
{
	/// <summary>
	/// Base64-encoded image content.
	/// </summary>
	public class ImageContent : Model.CodeContent.ImageContent, ICodeContentLatexRenderer
	{
		/// <summary>
		/// Base64-encoded image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates LaTeX for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderLatex(LatexRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;
			byte[] Bin = Convert.FromBase64String(GetImageBase64(Rows));

			int i = Language.IndexOf(':');
			string ContentType = i < 0 ? Language : Language.Substring(0, i);
			string Title = i < 0 ? string.Empty : Language.Substring(i + 1);

			if (!InternetContent.TryGetFileExtension(ContentType, out string Extension))
				Extension = "tmp";

			string FileName = await Multimedia.ImageContent.GetTemporaryFile(Bin, Extension);
			
			Output.AppendLine("\\begin{figure}[!hb]");
			Output.AppendLine("\\centering");

			Output.Append("\\fbox{\\includegraphics{");
			Output.Append(FileName.Replace('\\', '/'));
			Output.AppendLine("}}");

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("\\caption{");
				Output.Append(LatexRenderer.EscapeLaTeX(Title));
				Output.AppendLine("}");
			}

			Output.AppendLine("\\end{figure}");
			Output.AppendLine();

			return true;
		}
	}
}
