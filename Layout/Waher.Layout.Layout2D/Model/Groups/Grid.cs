using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Groups;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// A grid of cells
	/// </summary>
	public abstract class Grid : LayoutArea
	{
		private Cell[] cells;

		/// <summary>
		/// A grid of cells
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Grid(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.cells is null))
			{
				foreach (Cell E in this.cells)
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

			List<Cell> Children = new List<Cell>();

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = this.Document.CreateElement(E, this);

					if (Child is Cell Cell)
						Children.Add(Cell);
					else
						throw new LayoutSyntaxException("Not a cell: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}

			this.cells = Children.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.cells is null))
			{
				foreach (ILayoutElement Child in this.cells)
					Child.ToXml(Output);
			}
		}

	}
}
