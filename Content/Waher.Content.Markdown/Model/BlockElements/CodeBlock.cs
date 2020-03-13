using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a code block in a markdown document.
	/// </summary>
	public class CodeBlock : BlockElement
	{
		private readonly ICodeContent handler;
		private readonly string[] rows;
		private readonly string indentString;
		private readonly string language;
		private readonly int start, end, indent;


		/// <summary>
		/// Represents a code block in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Rows">Rows</param>
		/// <param name="Start">Start index of code.</param>
		/// <param name="End">End index of code.</param>
		/// <param name="Indent">Additional indenting.</param>
		public CodeBlock(MarkdownDocument Document, string[] Rows, int Start, int End, int Indent)
			: this(Document, Rows, Start, End, Indent, null)
		{
		}

		/// <summary>
		/// Represents a code block in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Rows">Rows</param>
		/// <param name="Start">Start index of code.</param>
		/// <param name="End">End index of code.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Language">Language used.</param>
		public CodeBlock(MarkdownDocument Document, string[] Rows, int Start, int End, int Indent, string Language)
			: base(Document)
		{
			this.rows = Rows;
			this.start = Start;
			this.end = End;
			this.indent = Indent;
			this.indentString = Indent <= 0 ? string.Empty : new string('\t', Indent);
			this.language = Language;
			this.handler = GetHandler(this.language);
		}

		private static ICodeContent[] codeContents = null;

		static CodeBlock()
		{
			Init();
			Types.OnInvalidated += (sender, e) => Init();
		}

		private static void Init()
		{
			List<ICodeContent> CodeContents = new List<ICodeContent>();
			TypeInfo TI;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICodeContent)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					ICodeContent CodeContent = (ICodeContent)Activator.CreateInstance(T);
					CodeContents.Add(CodeContent);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			lock (handlers)
			{
				codeContents = CodeContents.ToArray();
				handlers.Clear();
			}
		}

		private static ICodeContent GetHandler(string Language)
		{
			ICodeContent[] Handlers;

			if (string.IsNullOrEmpty(Language))
				return null;

			lock (handlers)
			{
				if (!handlers.TryGetValue(Language, out Handlers))
				{
					List<ICodeContent> List = new List<ICodeContent>();

					foreach (ICodeContent Content in codeContents)
					{
						if (Content.Supports(Language) > Grade.NotAtAll)
							List.Add(Content);
					}

					if (List.Count > 0)
						Handlers = List.ToArray();
					else
						Handlers = null;

					handlers[Language] = Handlers;
				}
			}

			if (Handlers is null)
				return null;

			ICodeContent Best = null;
			Grade BestGrade = Grade.NotAtAll;
			Grade ContentGrade;

			foreach (ICodeContent Content in Handlers)
			{
				ContentGrade = Content.Supports(Language);
				if (ContentGrade > BestGrade)
				{
					BestGrade = ContentGrade;
					Best = Content;
				}
			}

			return Best;
		}

		private readonly static Dictionary<string, ICodeContent[]> handlers = new Dictionary<string, ICodeContent[]>(StringComparer.CurrentCultureIgnoreCase);

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("```");
			Output.AppendLine(this.language);

			foreach (string Row in this.rows)
				Output.AppendLine(Row);

			Output.AppendLine("```");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			if (this.handler != null && this.handler.HandlesHTML)
			{
				try
				{
					if (this.handler.GenerateHTML(Output, this.rows, this.language, this.indent, this.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							Output.Append("<p><font style=\"color:red\">");
							Output.Append(XML.HtmlValueEncode(ex3.Message));
							Output.AppendLine("</font></p>");
						}
					}
					else
					{
						Output.Append("<p><font style=\"color:red\">");
						Output.Append(XML.HtmlValueEncode(ex.Message));
						Output.Append("</font></p>");
					}
				}
			}

			int i;

			Output.Append("<pre><code class=\"");

			if (string.IsNullOrEmpty(this.language))
				Output.Append("nohighlight");
			else
				Output.Append(XML.Encode(this.language));

			Output.Append("\">");

			for (i = this.start; i <= this.end; i++)
			{
				Output.Append(this.indentString);
				Output.AppendLine(XML.HtmlValueEncode(this.rows[i]));
			}

			Output.AppendLine("</code></pre>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			if (this.handler != null && this.handler.HandlesPlainText)
			{
				try
				{
					if (this.handler.GeneratePlainText(Output, this.rows, this.language, this.indent, this.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
							Output.AppendLine(ex3.Message);
					}
					else
						Output.AppendLine(ex.Message);
				}
			}

			int i;

			for (i = this.start; i <= this.end; i++)
			{
				Output.Append(this.indentString);
				Output.AppendLine(this.rows[i]);
			}

			Output.AppendLine();
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			if (this.handler != null && this.handler.HandlesXAML)
			{
				try
				{
					if (this.handler.GenerateXAML(Output, Settings, TextAlignment, this.rows, this.language, this.indent, this.Document))
						return;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							Output.WriteStartElement("TextBlock");
							Output.WriteAttributeString("TextWrapping", "Wrap");
							Output.WriteAttributeString("Margin", Settings.ParagraphMargins);

							if (TextAlignment != TextAlignment.Left)
								Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

							Output.WriteAttributeString("Foreground", "Red");
							Output.WriteValue(ex3.Message);
							Output.WriteEndElement();
						}
					}
					else
					{
						Output.WriteStartElement("TextBlock");
						Output.WriteAttributeString("TextWrapping", "Wrap");
						Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
						if (TextAlignment != TextAlignment.Left)
							Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

						Output.WriteAttributeString("Foreground", "Red");
						Output.WriteValue(ex.Message);
						Output.WriteEndElement();
					}
				}
			}

			bool First = true;

			Output.WriteStartElement("TextBlock");
			Output.WriteAttributeString("xml", "space", null, "preserve");
			Output.WriteAttributeString("TextWrapping", "NoWrap");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			Output.WriteAttributeString("FontFamily", "Courier New");
			if (TextAlignment != TextAlignment.Left)
				Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

			foreach (string Row in this.rows)
			{
				if (First)
					First = false;
				else
					Output.WriteElementString("LineBreak", string.Empty);

				Output.WriteValue(Row);
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// Code block indentation.
		/// </summary>
		public int Indent
		{
			get { return this.indent; }
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return false; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("CodeBlock");
			Output.WriteAttributeString("language", this.language);
			Output.WriteAttributeString("start", this.start.ToString());
			Output.WriteAttributeString("end", this.end.ToString());
			Output.WriteAttributeString("indent", this.indent.ToString());
			Output.WriteAttributeString("indentString", this.indentString);

			foreach (string s in this.rows)
				Output.WriteElementString("Row", s);

			Output.WriteEndElement();
		}
	}
}
