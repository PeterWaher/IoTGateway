using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a task list in a markdown document.
	/// </summary>
	public class TaskList : BlockElementChildren
	{
		/// <summary>
		/// Represents a task list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public TaskList(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a task list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public TaskList(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<ul class=\"taskList\">");

			foreach (MarkdownElement E in this.Children)
				E.GenerateHTML(Output);

			Output.AppendLine("</ul>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();
			string s;
			string s2 = Environment.NewLine + Environment.NewLine;
			bool LastIsParagraph = false;

			s = Output.ToString();
			if (!s.EndsWith(Environment.NewLine) && !string.IsNullOrEmpty(s))
				Output.AppendLine();

			foreach (MarkdownElement E in this.Children)
			{
				E.GeneratePlainText(sb);
				s = sb.ToString();
				sb.Clear();
				Output.Append(s);

				LastIsParagraph = s.EndsWith(s2);
			}

			if (!LastIsParagraph)
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
			int Row = 0;
			bool ParagraphBullet;

			Output.WriteStartElement("Grid");
			Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
			Output.WriteStartElement("Grid.ColumnDefinitions");

			Output.WriteStartElement("ColumnDefinition");
			Output.WriteAttributeString("Width", "Auto");
			Output.WriteEndElement();

			Output.WriteStartElement("ColumnDefinition");
			Output.WriteAttributeString("Width", "*");
			Output.WriteEndElement();

			Output.WriteEndElement();
			Output.WriteStartElement("Grid.RowDefinitions");

			foreach (MarkdownElement _ in this.Children)
			{
				Output.WriteStartElement("RowDefinition");
				Output.WriteAttributeString("Height", "Auto");
				Output.WriteEndElement();
			}

			Output.WriteEndElement();

			foreach (MarkdownElement E in this.Children)
			{
				ParagraphBullet = !E.InlineSpanElement || E.OutsideParagraph;
				E.GetMargins(Settings, out int TopMargin, out int BottomMargin);

				if (E is TaskItem TaskItem && TaskItem.IsChecked)
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
					Output.WriteAttributeString("Grid.Column", "0");
					Output.WriteAttributeString("Grid.Row", Row.ToString());
					if (TextAlignment != TextAlignment.Left)
						Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

					Output.WriteAttributeString("Margin", "0," + TopMargin.ToString() + "," +
						Settings.ListContentMargin.ToString() + "," + BottomMargin.ToString());

					Output.WriteValue("✓");
					Output.WriteEndElement();
				}

				Output.WriteStartElement("StackPanel");
				Output.WriteAttributeString("Grid.Column", "1");
				Output.WriteAttributeString("Grid.Row", Row.ToString());

				if (ParagraphBullet)
					E.GenerateXAML(Output, Settings, TextAlignment);
				else
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
					if (TextAlignment != TextAlignment.Left)
						Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

					E.GenerateXAML(Output, Settings, TextAlignment);

					Output.WriteEndElement();
				}

				Output.WriteEndElement();

				Row++;
			}

			Output.WriteEndElement();
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
			this.Export(Output, "TaskList");
		}
	}
}
