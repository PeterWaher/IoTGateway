using System.Threading.Tasks;
using Waher.Content.Markdown.Model;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Wpf.Multimedia
{
	/// <summary>
	/// Table of Contents.
	/// </summary>
	public class TableOfContents : Model.Multimedia.TableOfContents, IMultimediaWpfXamlRenderer
	{
		/// <summary>
		/// Table of Contents.
		/// </summary>
		public TableOfContents()
		{
		}

		/// <summary>
		/// Generates WPF XAML for the multimedia content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public Task RenderWpfXaml(WpfXamlRenderer Renderer, MultimediaItem[] Items, ChunkedList<MarkdownElement> ChildNodes, bool AloneInParagraph,
			MarkdownDocument Document)
		{
			return Task.CompletedTask;	// TODO
		}
	}
}
