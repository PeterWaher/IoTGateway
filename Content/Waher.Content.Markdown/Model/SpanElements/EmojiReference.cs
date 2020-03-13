using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Emoji;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Represents an Emoji.
	/// </summary>
	public class EmojiReference : MarkdownElement
	{
		private readonly EmojiInfo emoji;
		private readonly int level;
		private readonly string delimiter;

		/// <summary>
		/// Represents an Emoji.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Emoji">Emoji reference.</param>
		public EmojiReference(MarkdownDocument Document, EmojiInfo Emoji)
			: this(Document, Emoji, 1)
		{
		}

		/// <summary>
		/// Represents an Emoji.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Emoji">Emoji reference.</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		public EmojiReference(MarkdownDocument Document, EmojiInfo Emoji, int Level)
			: base(Document)
		{
			this.emoji = Emoji;
			this.level = Level;
			this.delimiter = Level == 1 ? ":" : new string(':', Level);
		}

		/// <summary>
		/// Emoji information.
		/// </summary>
		public EmojiInfo Emoji
		{
			get { return this.emoji; }
		}

		/// <summary>
		/// Level (number of colons used to define the emoji)
		/// </summary>
		public int Level
		{
			get { return this.level; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append(this.delimiter);
			Output.Append(this.emoji.ShortName);
			Output.Append(this.delimiter);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			IEmojiSource EmojiSource = this.Document.EmojiSource;

			if (EmojiSource is null)
			{
				Output.Append(this.delimiter);
				Output.Append(this.emoji.ShortName);
				Output.Append(this.delimiter);
			}
			else if (!EmojiSource.EmojiSupported(this.emoji))
				Output.Append(this.emoji.Unicode);
			else
				EmojiSource.GenerateHTML(Output, this.emoji, this.level, this.Document.Settings.EmbedEmojis);
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
			return this.delimiter + this.emoji.ShortName + this.delimiter;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			this.Document.EmojiSource.GetImageSource(this.emoji, this.level, out string Url, out int Width, out int Height);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Url);
			Output.WriteAttributeString("Width", Width.ToString());
			Output.WriteAttributeString("Height", Height.ToString());

			if (!string.IsNullOrEmpty(Emoji.Description))
				Output.WriteAttributeString("ToolTip", Emoji.Description);

			Output.WriteEndElement();
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph
		{
			get { return true; }
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Emoji");
			Output.WriteAttributeString("shortName", this.emoji.ShortName);

			if (this.level > 1)
				Output.WriteAttributeString("level", this.level.ToString());

			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is EmojiReference x &&
				this.emoji.ShortName == x.emoji.ShortName &&
				this.level == x.level &&
				this.delimiter == x.delimiter &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.emoji.ShortName.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.level.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.delimiter.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
