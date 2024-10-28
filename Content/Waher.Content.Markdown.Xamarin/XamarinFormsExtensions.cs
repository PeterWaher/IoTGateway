using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Xamarin
{
	/// <summary>
	/// Markdown rendering extensions for Xamarin.Forms XAML.
	/// </summary>
	public static class XamarinFormsExtensions
	{
		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <returns>Xamarin.Forms XAML</returns>
		public static Task<string> GenerateXamarinForms(this MarkdownDocument Document)
		{
			return Document.GenerateXamarinForms(XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <returns>Xamarin.Forms XAML</returns>
		public static async Task<string> GenerateXamarinForms(this MarkdownDocument Document, XmlWriterSettings XmlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateXamarinForms(Output, XmlSettings);
			return Output.ToString();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">Xamarin.Forms XAML will be output here.</param>
		public static Task GenerateXamarinForms(this MarkdownDocument Document, StringBuilder Output)
		{
			return Document.GenerateXamarinForms(Output, XML.WriterSettings(false, true));
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">Xamarin.Forms XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		public static Task GenerateXamarinForms(this MarkdownDocument Document, StringBuilder Output, XmlWriterSettings XmlSettings)
		{
			return Document.GenerateXamarinForms(Output, XmlSettings, new XamlSettings());
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">Xamarin.Forms XAML will be output here.</param>
		/// <param name="XmlSettings">XML settings.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		public static async Task GenerateXamarinForms(this MarkdownDocument Document, StringBuilder Output, XmlWriterSettings XmlSettings,
			XamlSettings XamlSettings)
		{
			XmlSettings.ConformanceLevel = ConformanceLevel.Fragment;

			using (XamarinFormsXamlRenderer Renderer = new XamarinFormsXamlRenderer(Output, XmlSettings, XamlSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Output">Xamarin.Forms XAML will be output here.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		public static async Task GenerateXamarinForms(this MarkdownDocument Document, StringBuilder Output, XamlSettings XamlSettings)
		{
			XmlWriterSettings XmlSettings = XML.WriterSettings(false, true);
			XmlSettings.ConformanceLevel = ConformanceLevel.Fragment;

			using (XamarinFormsXamlRenderer Renderer = new XamarinFormsXamlRenderer(Output, XmlSettings, XamlSettings))
			{
				await Document.RenderDocument(Renderer);
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML from the markdown text.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="XamlSettings">XAML Settings.</param>
		/// <returns>Xamarin.Forms XAML</returns>
		public static async Task<string> GenerateXamarinForms(this MarkdownDocument Document, XamlSettings XamlSettings)
		{
			StringBuilder Output = new StringBuilder();
			await Document.GenerateXamarinForms(Output, XamlSettings);
			return Output.ToString();
		}
	}
}
