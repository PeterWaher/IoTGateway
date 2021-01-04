using System;
using Waher.Content.Markdown.Model;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Type of markdown document.
	/// </summary>
	[Flags]
	public enum DocumentType
	{
		/// <summary>
		/// Empty document
		/// </summary>
		Empty = 1,

		/// <summary>
		/// Contains a single line containing a number.
		/// </summary>
		SingleNumber = 3,

		/// <summary>
		/// Contains a single line of text.
		/// </summary>
		SingleLine = 7,

		/// <summary>
		/// Contains a single paragraph of text.
		/// </summary>
		SingleParagraph = 15,

		/// <summary>
		/// Contains one code section.
		/// </summary>
		SingleCode = 17,

		/// <summary>
		/// Contains one table.
		/// </summary>
		SingleTable = 33,

		/// <summary>
		/// Contains complex content.
		/// </summary>
		Complex = 127
	}

	/// <summary>
	/// Information about a document.
	/// </summary>
	public class DocumentInformation
	{
		private readonly MarkdownDocument markdown;
		private readonly DocumentType type;
		private readonly string[] rows;

		/// <summary>
		/// Information about a document.
		/// </summary>
		public DocumentInformation(MarkdownDocument Markdown)
		{
			int i = 0;
			bool IsTable = false;
			bool IsCode = false;

			foreach (MarkdownElement E in Markdown.Elements)
			{
				i++;
				IsTable |= E is Model.BlockElements.Table;
				IsCode |= E is Model.BlockElements.CodeBlock;
			}

			this.markdown = Markdown;

			string s = Markdown.MarkdownText.Trim().Replace("\r\n", "\n").Replace('\r', '\n');
			this.rows = s.Split('\n');

			if (i == 0)
				this.type = DocumentType.Empty;
			else if (i == 1)
			{
				if (IsTable)
					this.type = DocumentType.SingleTable;
				else if (IsCode)
					this.type = DocumentType.SingleCode;
				else
				{
					if (string.IsNullOrEmpty(s))
						this.type = DocumentType.Empty;
					else
					{
						if (this.rows.Length > 1)
							this.type = DocumentType.SingleParagraph;
						else if (IsNumeric(this.rows[0]))
							this.type = DocumentType.SingleNumber;
						else
							this.type = DocumentType.SingleLine;
					}
				}
			}
			else
				this.type = DocumentType.Complex;
		}

		private static bool IsNumeric(string s)
		{
			if (s.StartsWith("<") && s.EndsWith(">"))
			{
				int i = s.IndexOf('>');
				int j = s.LastIndexOf('<');

				return (i < j && IsNumeric(s.Substring(i + 1, j - i - 1)));
			}
			else if (s.Length > 2 && s.StartsWith("`") && s.EndsWith("`"))
				return IsNumeric(s.Substring(1, s.Length - 2));
			else
				return CommonTypes.TryParse(s, out double _);
		}

		/// <summary>
		/// Markdown document.
		/// </summary>
		public MarkdownDocument Markdown => this.markdown;

		/// <summary>
		/// Document type.
		/// </summary>
		public DocumentType Type => this.type;

		/// <summary>
		/// Rows
		/// </summary>
		public string[] Rows => this.rows;
	}
}
