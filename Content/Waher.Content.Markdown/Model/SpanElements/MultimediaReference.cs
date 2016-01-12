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
		private bool aloneInParagraph;

		/// <summary>
		/// Multimedia reference.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Multimedia label.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		public MultimediaReference(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Label, bool AloneInParagraph)
			: base(Document, ChildElements, Label)
		{
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph
		{
			get { return this.aloneInParagraph; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.Label);

			if (Multimedia != null)
			{
				Multimedia.MultimediaHandler.GenerateHTML(Output, Multimedia.Url, Multimedia.Title, Multimedia.Width, Multimedia.Height,
					this.Children, this.aloneInParagraph, this.Document);
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			base.GeneratePlainText(Output);

			if (this.aloneInParagraph)
			{
				Output.AppendLine();
				Output.AppendLine();
			}
		}

		internal override bool OutsideParagraph
		{
			get
			{
				return true;
			}
		}
	
	}
}
