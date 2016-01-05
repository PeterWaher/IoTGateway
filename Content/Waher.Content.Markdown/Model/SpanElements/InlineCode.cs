using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineCode : MarkdownElementChildren
	{
		/// <summary>
		/// Inline source code.
		/// </summary>
		public InlineCode(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements)
			: base(Document, ChildElements)
		{
			LinkedListNode<MarkdownElement> Node;
			Text Text;

			if ((Node = ChildElements.First) != null && (Text = Node.Value as Text) != null && Text.Value.StartsWith(" "))
				Text.Value = Text.Value.Substring(1);

			if ((Node = ChildElements.Last) != null && (Text = Node.Value as Text) != null && Text.Value.EndsWith(" "))
				Text.Value = Text.Value.Substring(0, Text.Value.Length - 1);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<code>");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.Append("</code>");
		}

	}
}
