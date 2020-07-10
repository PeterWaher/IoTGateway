using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a task item in a task list.
	/// </summary>
	public class TaskItem : BlockElementSingleChild
	{
		private readonly int checkPosition;
		private readonly bool isChecked;

		/// <summary>
		/// Represents a task item in a task list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="IsChecked">If the item is checked or not.</param>
		/// <param name="CheckPosition">Position of the checkmark in the original markdown text document.</param>
		/// <param name="Child">Child element.</param>
		public TaskItem(MarkdownDocument Document, bool IsChecked, int CheckPosition, MarkdownElement Child)
			: base(Document, Child)
		{
			this.isChecked = IsChecked;
			this.checkPosition = CheckPosition;
		}

		/// <summary>
		/// If the item is checked or not.
		/// </summary>
		public bool IsChecked
		{
			get { return this.isChecked; }
		}

		/// <summary>
		/// Position of the checkmark in the original markdown text document.
		/// </summary>
		public int CheckPosition
		{
			get { return this.checkPosition; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			PrefixedBlock(Output, this.Child, this.isChecked ? "[x]\t" : "[ ]\t", "\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li class=\"taskListItem\"><input disabled=\"disabled");
			
			if (this.checkPosition > 0)
			{
				Output.Append("\" id=\"item");
				Output.Append(this.checkPosition.ToString());
				Output.Append("\" data-position=\"");
				Output.Append(this.checkPosition.ToString());
			}

			Output.Append("\" type=\"checkbox\"");

			if (this.isChecked)
				Output.Append(" checked=\"checked\"");

			Output.Append("/><span></span><label class=\"taskListItemLabel\"");

			if (this.checkPosition > 0)
			{
				Output.Append(" for=\"item");
				Output.Append(this.checkPosition.ToString());
				Output.Append("\"");
			}

			Output.Append('>');

			if (this.Child is NestedBlock NestedBlock)
			{
				bool EndLabel = true;
				bool First = true;

				foreach (MarkdownElement E in NestedBlock.Children)
				{
					if (First)
					{
						First = false;

						if (E.InlineSpanElement)
							E.GenerateHTML(Output);
						else
						{
							NestedBlock.GenerateHTML(Output);
							break;
						}
					}
					else
					{
						if (!E.InlineSpanElement)
						{
							Output.Append("</label>");
							EndLabel = false;
						}

						E.GenerateHTML(Output);
					}
				}

				if (EndLabel)
					Output.Append("</label>");

				Output.AppendLine("</li>");
			}
			else
			{
				this.Child.GenerateHTML(Output);
				Output.AppendLine("</label></li>");
			}
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
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.Child.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.Child.GenerateXamarinForms(Output, TextAlignment);
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
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal override void GetMargins(out int TopMargin, out int BottomMargin)
		{
			this.Child.GetMargins(out TopMargin, out BottomMargin);
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

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Child"/>.
		/// </summary>
		/// <param name="Child">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementSingleChild Create(MarkdownElement Child, MarkdownDocument Document)
		{
			return new TaskItem(Document, this.isChecked, this.checkPosition, Child);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is TaskItem x &&
				x.isChecked == this.isChecked &&
				x.checkPosition == this.checkPosition &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is TaskItem x &&
				this.isChecked == x.isChecked &&
				this.checkPosition == x.checkPosition &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.isChecked.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.checkPosition.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
