using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// Inline comment found in the document.
	/// </summary>
	public class Comment : HtmlNode
    {
		private string comment;

		/// <summary>
		/// Inline comment found in the document.
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="Comment">Comment</param>
		public Comment(HtmlDocument Document, HtmlElement Parent, int StartPosition,
			int EndPosition, string Comment)
			: base(Document, Parent, StartPosition, EndPosition)
		{
			this.comment = Comment;
		}

		/// <summary>
		/// Comment
		/// </summary>
		public string CommentText
		{
			get { return this.comment; }
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteComment(this.comment);
		}
	}
}
