using System;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Graphs;

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
		/// Contains one graph.
		/// </summary>
		SingleGraph = 65,

		/// <summary>
		/// Contains one block of XML
		/// </summary>
		SingleXml = 129,

		/// <summary>
		/// Contains complex content.
		/// </summary>
		Complex = 511
	}

	/// <summary>
	/// Information about a document.
	/// </summary>
	public class DocumentInformation
	{
		private readonly MarkdownDocument markdown;
		private readonly DocumentType type;
		private readonly string[] rows;
		private readonly Type graphType;
		private readonly Graph graph;
		private readonly Table table;

		/// <summary>
		/// Information about a document.
		/// </summary>
		public DocumentInformation(MarkdownDocument Markdown)
		{
			MarkdownElement First = null;
			int i = 0;
			bool IsTable = false;
			bool IsCode = false;

			foreach (MarkdownElement E in Markdown.Elements)
			{
				if (First is null)
					First = E;

				i++;
				IsTable |= E is Table;
				IsCode |= E is CodeBlock;
			}

			this.markdown = Markdown;

			string s = Markdown.MarkdownText.Trim().Replace("\r\n", "\n").Replace('\r', '\n');
			this.rows = s.Split('\n');

			if (i == 0)
				this.type = DocumentType.Empty;
			else if (i == 1)
			{
				int c = this.rows.Length;

				if (IsTable)
				{
					this.table = (Table)First;
					this.type = DocumentType.SingleTable;
				}
				else if (IsCode)
				{
					if (c >= 3 &&
						this.rows[0].StartsWith("```", StringComparison.CurrentCultureIgnoreCase) &&
						this.rows[1].StartsWith("<", StringComparison.CurrentCultureIgnoreCase) &&
						this.rows[c - 2].EndsWith(">", StringComparison.CurrentCultureIgnoreCase) &&
						this.rows[c - 1].EndsWith("```"))
					{
						if (c >= 4 &&
							this.rows[0].StartsWith("```Graph", StringComparison.CurrentCultureIgnoreCase) &&
							this.rows[1].StartsWith("<Graph", StringComparison.CurrentCultureIgnoreCase) &&
							this.rows[c - 2].EndsWith("</Graph>", StringComparison.CurrentCultureIgnoreCase) &&
							this.rows[c - 1].EndsWith("```"))
						{
							try
							{
								StringBuilder sb = new StringBuilder();

								for (i = 1, c--; i < c; i++)
									sb.AppendLine(Rows[i]);

								XmlDocument Xml = new XmlDocument();
								Xml.LoadXml(sb.ToString());

								if (!(Xml.DocumentElement is null) &&
									Xml.DocumentElement.LocalName == Graph.GraphLocalName &&
									Xml.DocumentElement.NamespaceURI == Graph.GraphNamespace)
								{
									string TypeName = XML.Attribute(Xml.DocumentElement, "type");
									this.graphType = Types.GetType(TypeName);
									if (this.graphType is null)
										this.type = DocumentType.SingleXml;
									else
									{
										this.graph = (Graph)Activator.CreateInstance(this.graphType);
										this.graph.SameScale = XML.Attribute(Xml.DocumentElement, "sameScale", false);

										foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
										{
											if (N is XmlElement E)
											{
												this.graph.ImportGraph(E);
												break;
											}
										}

										this.type = DocumentType.SingleGraph;
									}
								}
								else
									this.type = DocumentType.SingleXml;
							}
							catch (Exception)
							{
								this.type = DocumentType.SingleXml;
							}
						}
						else
							this.type = DocumentType.SingleXml;
					}
					else
						this.type = DocumentType.SingleCode;
				}
				else
				{
					if (string.IsNullOrEmpty(s))
						this.type = DocumentType.Empty;
					else
					{
						if (c > 1)
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
		/// Graph object, if <see cref="DocumentType.SingleGraph"/>
		/// </summary>
		public Graph Graph => this.graph;

		/// <summary>
		/// Table object, if <see cref="DocumentType.SingleTable"/>
		/// </summary>
		public Table Table => this.table;

		/// <summary>
		/// Rows
		/// </summary>
		public string[] Rows => this.rows;
	}
}
