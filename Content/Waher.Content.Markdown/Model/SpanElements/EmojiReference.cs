using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Emoji;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an Emoji.
	/// </summary>
	public class EmojiReference : MarkdownElement
	{
		private EmojiInfo emoji;

		/// <summary>
		/// Represents an Emoji.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Emoji">Emoji reference.</param>
		public EmojiReference(MarkdownDocument Document, EmojiInfo Emoji)
			: base(Document)
		{
			this.emoji = Emoji;
		}

		/// <summary>
		/// Emoji information.
		/// </summary>
		public EmojiInfo Emoji
		{
			get { return this.emoji; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			IEmojiSource EmojiSource = this.Document.EmojiSource;

			if (EmojiSource == null)
			{
				Output.Append(':');
				Output.Append(this.emoji.ShortName);
				Output.Append(':');
			}
			else if (!EmojiSource.EmojiSupported(this.emoji))
				Output.Append(this.emoji.Unicode);
			else
				EmojiSource.GenerateHTML(Output, this.emoji);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.emoji.Unicode);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return ":" + this.emoji.ShortName + ":";
		}
	}
}
