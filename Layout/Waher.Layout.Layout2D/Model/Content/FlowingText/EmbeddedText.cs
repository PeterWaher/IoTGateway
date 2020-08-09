using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Content.FlowingText;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Abstract base class of embedded text elements in flowing text.
	/// </summary>
	public abstract class EmbeddedText : LayoutElement, IFlowingText
	{
		private IFlowingText[] text;

		/// <summary>
		/// Abstract base class of embedded text elements in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EmbeddedText(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

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
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is EmbeddedText Dest)
			{
				if (!(this.text is null))
				{
					int i, c = this.text.Length;

					IFlowingText[] Children = new IFlowingText[c];

					for (i = 0; i < c; i++)
						Children[i] = this.text[i].Copy(Dest) as IFlowingText;

					Dest.text = Children;
				}
			}
		}

	}
}
