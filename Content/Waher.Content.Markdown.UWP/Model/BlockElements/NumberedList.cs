﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a numbered list in a markdown document.
	/// </summary>
	public class NumberedList : MarkdownElementChildren
	{
		/// <summary>
		/// Represents a numbered list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NumberedList(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Represents a numbered list in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public NumberedList(MarkdownDocument Document, params MarkdownElement[] Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			NumberedItem Item;
			int Expected = 0;

			Output.AppendLine("<ol>");

			foreach (MarkdownElement E in this.Children)
			{
				Expected++;
				Item = E as NumberedItem;

				if (Item == null)
					E.GenerateHTML(Output);
				else if (Item.Number == Expected)
				{
					Output.Append("<li>");
					Item.Child.GenerateHTML(Output);
					Output.AppendLine("</li>");
				}
				else
				{
					Item.GenerateHTML(Output);
					Expected = Item.Number;
				}
			}

			Output.AppendLine("</ol>");
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
			NumberedItem Item;
			int Expected = 0;
			int Row = 0;
			int TopMargin;
			int BottomMargin;
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

			foreach (MarkdownElement E in this.Children)
			{
				Output.WriteStartElement("RowDefinition");
				Output.WriteAttributeString("Height", "Auto");
				Output.WriteEndElement();
			}

			Output.WriteEndElement();

			foreach (MarkdownElement E in this.Children)
			{
				Expected++;
				Item = E as NumberedItem;

				ParagraphBullet = !E.InlineSpanElement || E.OutsideParagraph;
				E.GetMargins(Settings, out TopMargin, out BottomMargin);

				Output.WriteStartElement("TextBlock");
				Output.WriteAttributeString("TextWrapping", "Wrap");
				Output.WriteAttributeString("Grid.Column", "0");
				Output.WriteAttributeString("Grid.Row", Row.ToString());
				if (TextAlignment != TextAlignment.Left)
					Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

				Output.WriteAttributeString("Margin", "0," + TopMargin.ToString() + "," + 
					Settings.ListContentMargin.ToString() + "," + BottomMargin.ToString());

				if (Item != null)
					Output.WriteValue((Expected = Item.Number).ToString());
				else
					Output.WriteValue(Expected.ToString());

				Output.WriteValue(".");
				Output.WriteEndElement();

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
	}
}
