using System;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Xamarin XAML Rendering State.
	/// </summary>
	public class XamarinRenderingState
	{
		/// <summary>
		/// Text alignment
		/// </summary>
		public TextAlignment TextAlignment = TextAlignment.Left;

		/// <summary>
		/// If text is bold
		/// </summary>
		public bool Bold = false;

		/// <summary>
		/// If text is italic
		/// </summary>
		public bool Italic = false;

		/// <summary>
		/// If text is stricken through
		/// </summary>
		public bool StrikeThrough = false;

		/// <summary>
		/// If text is underlined
		/// </summary>
		public bool Underline = false;

		/// <summary>
		/// If text is superscript
		/// </summary>
		public bool Superscript = false;

		/// <summary>
		/// If text is subscript
		/// </summary>
		public bool Subscript = false;

		/// <summary>
		/// If text is inline code.
		/// </summary>
		public bool Code = false;

		/// <summary>
		/// If rendering is inside a label.
		/// </summary>
		public bool InLabel = false;

		/// <summary>
		/// Link, if rendering a hyperlink, null otherwise.
		/// </summary>
		public string Hyperlink = null;
	}
}
