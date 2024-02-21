using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Wpf
{
	/// <summary>
	/// Markdown rendering extensions for WPF XAML.
	/// </summary>
	public static class WpfExtensions
	{
		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <returns>WPF XAML</returns>
		public static Task<string> GenerateXAML(this MarkdownDocument Document)
		{
			return Document.GenerateXAML(XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <returns>WPF XAML</returns>
		public static async Task<string> GenerateXAML(this MarkdownDocument Document, XmlWriterSettings XmlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateXAML(Output, XmlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">WPF XAML will be output here.</param>
		public static Task GenerateXAML(this MarkdownDocument Document, StringBuilder Output)
		{
			return Document.GenerateXAML(Output, XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">WPF XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		public static Task GenerateXAML(this MarkdownDocument Document, StringBuilder Output, XmlWriterSettings XmlSettings)
		{
			return Document.GenerateXAML(Output, XmlSettings, new XamlSettings());
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">WPF XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		public static async Task GenerateXAML(this MarkdownDocument Document, StringBuilder Output, XmlWriterSettings XmlSettings,
			XamlSettings XamlSettings)
		{
			using (WpfXamlRenderer Renderer = new WpfXamlRenderer(Output, XmlSettings, XamlSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">WPF XAML will be output here.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		public static async Task GenerateXAML(this MarkdownDocument Document, StringBuilder Output, XamlSettings XamlSettings)
		{
			using (WpfXamlRenderer Renderer = new WpfXamlRenderer(Output, XML.WriterSettings(false, true), XamlSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates WPF XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		/// <returns>WPF XAML</returns>
		public static async Task<string> GenerateXAML(this MarkdownDocument Document, XamlSettings XamlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateXAML(Output, XamlSettings);
			return Output.ToString();
		}
	}
}
