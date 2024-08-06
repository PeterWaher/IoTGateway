using Waher.Runtime.Inventory;
using System.Threading.Tasks;
using Waher.Content.Markdown.Contracts;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to Neuro-Foundation Smart Contract XML documents.
	/// </summary>
	public class MarkdownToSmartContractConverter : MarkdownToHtmlConverter
	{
		/// <summary>
		/// Converts Markdown documents to Neuro-Foundation Smart Contract XML documents.
		/// </summary>
		public MarkdownToSmartContractConverter()
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
					"application/xml+iotsc"
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
			return Doc.GenerateSmartContractXml();
		}

	}
}
