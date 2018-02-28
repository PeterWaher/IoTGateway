using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a task item in a task list.
	/// </summary>
	public class TaskItem : MarkdownElementSingleChild
	{
		private bool isChecked;

		/// <summary>
		/// Represents a task item in a task list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="IsChecked">If the item is checked or not.</param>
		/// <param name="Child">Child element.</param>
		public TaskItem(MarkdownDocument Document, bool IsChecked, MarkdownElement Child)
			: base(Document, Child)
		{
			this.isChecked = IsChecked;
		}

		/// <summary>
		/// If the item is checked or not.
		/// </summary>
		public bool IsChecked
		{
			get { return this.isChecked; }
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li class=\"taskListItem\"><label class=\"taskListItemLabel\"><input disabled=\"disabled\" type=\"checkbox\"");

			if (this.isChecked)
				Output.Append(" checked=\"checked\"");

			Output.Append("/></label>");
			
			this.Child.GenerateHTML(Output);

			Output.AppendLine("</li>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			if (this.isChecked)
				Output.Append("[X] ");
			else
				Output.Append("[ ] ");

			StringBuilder sb = new StringBuilder();
			this.Child.GeneratePlainText(sb);

			string s = sb.ToString();

			Output.Append(s);

			if (!s.EndsWith("\n"))
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
			this.Child.GenerateXAML(Output, Settings, TextAlignment);
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get
			{
				return this.Child.InlineSpanElement;
			}
		}

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal override void GetMargins(XamlSettings Settings, out int TopMargin, out int BottomMargin)
		{
			this.Child.GetMargins(Settings, out TopMargin, out BottomMargin);
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("TaskItem");
			Output.WriteAttributeString("isChecked", CommonTypes.Encode(this.isChecked));
			this.ExportChild(Output);
			Output.WriteEndElement();
		}
	}
}
