using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for layout containers (area elements containing 
	/// embedded layout elements).
	/// </summary>
	public abstract class LayoutContainer : LayoutArea
	{
		private ILayoutElement[] children;

		/// <summary>
		/// Abstract base class for layout containers (area elements containing 
		/// embedded layout elements).
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LayoutContainer(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Child elements
		/// </summary>
		public ILayoutElement[] Children => this.children;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.children is null))
			{
				foreach (ILayoutElement E in this.children)
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

			List<ILayoutElement> Children = null;

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					if (Children is null)
						Children = new List<ILayoutElement>();

					Children.Add(this.Document.CreateElement(E, this));
				}
			}

			this.children = Children?.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.children is null))
			{
				foreach (ILayoutElement Child in this.children)
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

			if (Destination is LayoutContainer Dest)
			{
				if (!(this.children is null))
				{
					int i, c = this.children.Length;

					ILayoutElement[] Children = new ILayoutElement[c];

					for (i = 0; i < c; i++)
						Children[i] = this.children[i].Copy(Dest);

					Dest.children = Children;
				}
			}
		}

	}
}
