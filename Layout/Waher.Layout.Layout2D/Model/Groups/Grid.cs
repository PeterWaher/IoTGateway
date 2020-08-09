using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// A grid of cells
	/// </summary>
	public class Grid : SpatialDistribution
	{
		private PositiveIntegerAttribute columns;

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
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Grid";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.columns = new PositiveIntegerAttribute(Input, "columns");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.columns.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Grid(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Grid Dest)
				Dest.columns = this.columns.CopyIfNotPreset();
		}

		/// <summary>
		/// Gets a cell layout object that will be responsible for laying out cells.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Cell layout object.</returns>
		public override ICellLayout GetCellLayout(DrawingState State)
		{
			if (!this.columns.TryEvaluate(State.Session, out int NrColumns))
				NrColumns = 1;

			return new GridCells(State.Session, NrColumns);
		}

	}
}
