using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to XAML documents.
	/// </summary>
	public class MarkdownToXamlConverter : MarkdownToHtmlConverter
	{
		/// <summary>
		/// Converts Markdown documents to XAML documents.
		/// </summary>
		public MarkdownToXamlConverter()
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
					"application/xml+xaml"
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
			return Doc.GenerateXAML();
		}

	}
}
