using System;
using System.Text;
using System.Threading.Tasks;

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
	}
}
