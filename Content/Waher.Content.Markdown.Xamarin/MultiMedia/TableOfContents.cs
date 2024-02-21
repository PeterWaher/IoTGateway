using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Xamarin.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : Model.Multimedia.TableOfContents, IMultimediaXamarinFormsXamlRenderer
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			return Task.CompletedTask;	// TODO
		}
	}
}
