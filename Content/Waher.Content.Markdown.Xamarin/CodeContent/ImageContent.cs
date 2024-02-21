using System.Threading.Tasks;
using Waher.Content.Emoji;

namespace Waher.Content.Markdown.Xamarin.CodeContent
{
	/// <summary>
	/// Base64-encoded image content.
	/// </summary>
	public class ImageContent : Model.CodeContent.ImageContent, ICodeContentXamarinFormsXamlRenderer
	{
		/// <summary>
		/// Base64-encoded image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, string[] Rows, string Language, int Indent, 
			MarkdownDocument Document)
		{
			await Multimedia.ImageContent.OutputXamarinForms(Renderer.XmlOutput, new ImageSource()
			{
				Url = GenerateUrl(Language, Rows, out _, out _)
			});

			return true;
		}
	}
}
