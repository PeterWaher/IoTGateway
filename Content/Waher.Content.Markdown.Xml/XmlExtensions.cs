using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Xml
{
	/// <summary>
	/// Markdown rendering extensions for Xamarin.Forms XAML.
	/// </summary>
	public static class XmlExtensions
	{
		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <returns>XML String.</returns>
		public static async Task<string> ExportXml(this MarkdownDocument Document)
		{
			StringBuilder Xml = new StringBuilder();
			await Document.ExportXml(Xml);
			return Xml.ToString();
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Xml">XML Output.</param>
		public static Task ExportXml(this MarkdownDocument Document, StringBuilder Xml)
		{
			return Document.ExportXml(Xml, XML.WriterSettings(true, true));
		}

		/// <summary>
		/// Exports the parsed document to XML.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Xml">XML Output.</param>
		/// <param name="Settings">XML Settings.</param>
		public static async Task ExportXml(this MarkdownDocument Document, StringBuilder Xml, XmlWriterSettings Settings)
		{
			using (XmlRenderer Renderer = new XmlRenderer(Xml, Settings))
			{
				await Document.RenderDocument(Renderer);
			}
		}
	}
}
