using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Markdown output type.
	/// </summary>
	public enum MarkdownOutputType
	{
		/// <summary>
		/// Plaint text
		/// </summary>
		PlainText,

		/// <summary>
		/// Markdown
		/// </summary>
		Markdown,

		/// <summary>
		/// HTML
		/// </summary>
		Html,

		/// <summary>
		/// XAML (WPF version)
		/// </summary>
		Xaml,

		/// <summary>
		/// XAML (Xamarin.Forms version)
		/// </summary>
		XamarinForms,

		/// <summary>
		/// Smart Contract XML
		/// </summary>
		SmartContract,

		/// <summary>
		/// LaTeX
		/// </summary>
		Latex,

		/// <summary>
		/// XML
		/// </summary>
		Xml
	}

	/// <summary>
	/// Interface for classes that help output asynchronous markdown output.
	/// </summary>
	public interface IMarkdownAsynchronousOutput : IProcessingSupport<MarkdownOutputType>
	{
		/// <summary>
		/// Generates a stub in the output, that will be filled with the asynchronously generated
		/// content, once it is reported.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Output">Output being generated.</param>
		/// <param name="Title">Title of content.</param>
		/// <returns>ID to report back, when content is completed.</returns>
		Task<string> GenerateStub(MarkdownOutputType Type, StringBuilder Output, string Title);

		/// <summary>
		/// Method called when asynchronous result has been generated in a Markdown document.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Id">ID of generated content.</param>
		/// <param name="Result">Generated content.</param>
		Task ReportResult(MarkdownOutputType Type, string Id, string Result);

		/// <summary>
		/// Method called when asynchronous result has been generated in a Markdown document.
		/// </summary>
		/// <param name="Type">Output type.</param>
		/// <param name="Id">ID of generated content.</param>
		/// <param name="Result">Generated content.</param>
		/// <param name="More">If more information will be sent in another call.</param>
		Task ReportResult(MarkdownOutputType Type, string Id, string Result, bool More);
	}
}
