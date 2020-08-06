using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Defines a cell in a grid.
	/// </summary>
	public class Cell : LayoutContainer
	{
		private PositiveIntegerAttribute column;
		private PositiveIntegerAttribute row;
		private PositiveIntegerAttribute colSpan;
		private PositiveIntegerAttribute rowSpan;

		/// <summary>
		/// Defines a cell in a grid.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Cell(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Cell";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.column = new PositiveIntegerAttribute(Input, "column");
			this.row = new PositiveIntegerAttribute(Input, "row");
			this.colSpan = new PositiveIntegerAttribute(Input, "colSpan");
			this.rowSpan = new PositiveIntegerAttribute(Input, "rowSpan");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.column.Export(Output);
			this.row.Export(Output);
			this.colSpan.Export(Output);
			this.rowSpan.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Cell(Document, Parent);
		}
	}
}
