using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia reference
	/// </summary>
	public class MultimediaReference : MarkdownElementChildren
	{
		private string label;

		/// <summary>
		/// Multimedia
		/// </summary>
		public MultimediaReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label)
			: base(Document, ChildElements)
		{
			this.label = Label;
		}

		/// <summary>
		/// Multimedia label
		/// </summary>
		private string Label
		{
			get { return this.label; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (Multimedia != null)
				Multimedia.MultimediaHandler.GenerateHTML(Output, Multimedia.Url, Multimedia.Title, Multimedia.Width, Multimedia.Height, this.Children);
		}
	
	}
}
