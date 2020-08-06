using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Content.FlowingText;

namespace Waher.Layout.Layout2D.Model.Content
{
	/// <summary>
	/// Represents a paragraph of flowing text.
	/// </summary>
	public class Paragraph : LayoutArea
	{
		private IFlowingText[] text;

		/// <summary>
		/// Represents a paragraph of flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Paragraph(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Paragraph";

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.text is null))
			{
				foreach (ILayoutElement E in this.text)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			List<IFlowingText> Children = new List<IFlowingText>();

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = this.Document.CreateElement(E, this);

					if (Child is IFlowingText Text)
						Children.Add(Text);
					else
						throw new LayoutSyntaxException("Not flowing text: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}

			this.text = Children.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.text is null))
			{
				foreach (ILayoutElement Child in this.text)
					Child.ToXml(Output);
			}
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Paragraph(Document, Parent);
		}
	}
}
