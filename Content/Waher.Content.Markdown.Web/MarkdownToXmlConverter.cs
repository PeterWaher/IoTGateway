using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to XML documents.
	/// </summary>
	public class MarkdownToXmlConverter : MarkdownToHtmlConverter
	{
		/// <summary>
		/// Converts Markdown documents to XML documents.
		/// </summary>
		public MarkdownToXmlConverter()
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
					"text/xml",
					"application/xml"
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
			return Task.FromResult(Doc.ExportXml());
		}

	}
}
