using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to plain text documents.
	/// </summary>
	public class MarkdownToTextConverter : MarkdownToHtmlConverter
	{
		/// <summary>
		/// Converts Markdown documents to plain text documents.
		/// </summary>
		public MarkdownToTextConverter()
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
					"text/plain"
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
			return Doc.GeneratePlainText();
		}

	}
}
