using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Meta-data reference
	/// </summary>
	public class MetaReference : MarkdownElement
	{
		private readonly string key;

		/// <summary>
		/// Meta-data reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Key">Meta-data key.</param>
		public MetaReference(MarkdownDocument Document, string Key)
			: base(Document)
		{
			this.key = Key;
		}

		/// <summary>
		/// Meta-data key
		/// </summary>
		public string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("[%");
			Output.Append(this.key);
			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			bool FirstOnRow = true;

			if (this.Document.TryGetMetaData(this.key, out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						Output.Append(' ');

					Output.Append(XML.HtmlValueEncode(P.Key));
					if (P.Value)
					{
						Output.AppendLine("<br/>");
						FirstOnRow = true;
					}
				}
			}
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			bool FirstOnRow = true;

			if (this.Document.TryGetMetaData(this.key, out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						Output.Append(' ');

					Output.Append(P.Key);
					if (P.Value)
					{
						Output.AppendLine();
						FirstOnRow = true;
					}
				}
			}
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.key;
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			bool FirstOnRow = true;

			if (this.Document.TryGetMetaData(this.key, out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						Output.WriteValue(' ');

					Output.WriteValue(P.Key);
					if (P.Value)
					{
						Output.WriteElementString("LineBreak", string.Empty);
						FirstOnRow = true;
					}
				}
			}
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
			Output.WriteStartElement("MetaReference");
			Output.WriteAttributeString("key", this.key);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is MetaReference x &&
				this.key == x.key &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.key?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
