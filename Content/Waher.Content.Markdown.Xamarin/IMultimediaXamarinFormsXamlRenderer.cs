using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Xamarin
{
	/// <summary>
	/// Interface for multimedia content Xamarin.Forms XAML renderers.
	/// </summary>
	public interface IMultimediaXamarinFormsXamlRenderer : IMultimediaRenderer
	{
		/// <summary>
		/// Generates Xamarin.Forms XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, 
			bool AloneInParagraph, MarkdownDocument Document);
	}
}
