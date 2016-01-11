using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Meta-data reference
	/// </summary>
	public class MetaReference : MarkdownElement
	{
		private string key;

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
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			KeyValuePair<string, bool>[] Values;
			bool FirstOnRow = true;

			if (this.Document.TryGetMetaData(this.key, out Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						Output.Append(' ');

					Output.Append(MarkdownDocument.HtmlValueEncode(P.Key));
					if (P.Value)
					{
						Output.AppendLine("<br/>");
						FirstOnRow = true;
					}
				}
			}
		}

		public override void GeneratePlainText(StringBuilder Output)
		{
			KeyValuePair<string, bool>[] Values;
			bool FirstOnRow = true;

			if (this.Document.TryGetMetaData(this.key, out Values))
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
	}
}
