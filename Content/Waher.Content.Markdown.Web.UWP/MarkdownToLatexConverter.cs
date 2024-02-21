using Waher.Runtime.Inventory;
using System.Threading.Tasks;
using Waher.Content.Markdown.Latex;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to LaTeX documents.
	/// </summary>
	public class MarkdownToLatexConverter : MarkdownToHtmlConverter
	{
		/// <summary>
		/// Converts Markdown documents to LaTeX documents.
		/// </summary>
		public MarkdownToLatexConverter()
		{
		}

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public override string[] ToContentTypes
		{
			get
			{
				return new string[]
				{
					"text/x-tex",
					"application/x-tex"
				};
			}
		}

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public override Grade ConversionGrade => Grade.Ok;

		/// <summary>
		/// Performs the actual conversion
		/// </summary>
		/// <param name="Doc">Markdown document prepared for conversion.</param>
		/// <returns>Conversion result.</returns>
		protected override Task<string> DoConversion(MarkdownDocument Doc)
		{
			return Doc.GenerateLaTeX();
		}

	}
}
