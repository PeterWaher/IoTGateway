using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia reference
	/// </summary>
	public class MultimediaReference : LinkReference
	{
		/// <summary>
		/// Multimedia reference.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Multimedia label.</param>
		public MultimediaReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label)
			: base(Document, ChildElements, Label)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.Label);

			if (Multimedia != null)
				Multimedia.MultimediaHandler.GenerateHTML(Output, Multimedia.Url, Multimedia.Title, Multimedia.Width, Multimedia.Height, this.Children);
		}
	
	}
}
