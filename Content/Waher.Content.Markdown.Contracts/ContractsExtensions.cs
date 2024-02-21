using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Contracts
{
	/// <summary>
	/// Markdown rendering extensions for Smart Contracts.
	/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
	/// </summary>
	public static class ContractsExtensions
	{
		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <returns>Smart Contract XML</returns>
		public static Task<string> GenerateSmartContractXml(this MarkdownDocument Document)
		{
			return Document.GenerateSmartContractXml(XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Document">Markdown document being rendered.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <returns>Smart Contract XML</returns>
		public static async Task<string> GenerateSmartContractXml(this MarkdownDocument Document, XmlWriterSettings XmlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateSmartContractXml(Output, XmlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Document">Markdown document being rendered.</param>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		public static Task GenerateSmartContractXml(this MarkdownDocument Document, StringBuilder Output)
		{
			return Document.GenerateSmartContractXml(Output, XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Document">Markdown document being rendered.</param>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		public static async Task GenerateSmartContractXml(this MarkdownDocument Document, StringBuilder Output, XmlWriterSettings XmlSettings)
		{
			XmlSettings.ConformanceLevel = ConformanceLevel.Fragment;

			using (ContractsRenderer Renderer = new ContractsRenderer(Output, XmlSettings, null))
			{
				await Document.RenderDocument(Renderer);
			}
		}
	}
}
