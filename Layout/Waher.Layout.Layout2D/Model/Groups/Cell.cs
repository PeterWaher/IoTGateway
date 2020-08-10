using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Horizontal alignment
	/// </summary>
	public enum HorizontalAlignment
	{
		/// <summary>
		/// Aligned to the left
		/// </summary>
		Left,

		/// <summary>
		/// Aligned along centers
		/// </summary>
		Center,

		/// <summary>
		/// Aligned to the right
		/// </summary>
		Right
	}

	/// <summary>
	/// Vertical alignment
	/// </summary>
	public enum VerticalAlignment
	{
		/// <summary>
		/// Aligned at the top
		/// </summary>
		Top,

		/// <summary>
		/// Aligned along centers
		/// </summary>
		Center,

		/// <summary>
		/// Aligned at the bottom
		/// </summary>
		Bottom
	}

	/// <summary>
	/// Defines a cell in a grid.
	/// </summary>
	public class Cell : LayoutContainer
	{
		private PositiveIntegerAttribute colSpan;
		private PositiveIntegerAttribute rowSpan;
		private EnumAttribute<HorizontalAlignment> halign;
		private EnumAttribute<VerticalAlignment> valign;

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

			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign");
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign");
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

			this.halign.Export(Output);
			this.valign.Export(Output);
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

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Cell Dest)
			{
				Dest.halign = this.halign.CopyIfNotPreset();
				Dest.valign = this.valign.CopyIfNotPreset();
				Dest.colSpan = this.colSpan.CopyIfNotPreset();
				Dest.rowSpan = this.rowSpan.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Aligns a measured cell
		/// </summary>
		/// <param name="MaxWidth">Maximum width of area assigned to the cell</param>
		/// <param name="MaxHeight">Maximum height of area assigned to the cell</param>
		/// <param name="OffsetX">X-offset of cell.</param>
		/// <param name="OffsetY">Y-offset of cell.</param>
		/// <param name="Session">Current session.</param>
		public void AlignedMeasuredCell(double? MaxWidth, double? MaxHeight, ref double OffsetX, ref double OffsetY,
			Variables Session)
		{
			if (MaxWidth.HasValue && this.halign.TryEvaluate(Session, out HorizontalAlignment HAlignment) &&
				HAlignment != HorizontalAlignment.Left)
			{
				if (HAlignment == HorizontalAlignment.Right)
					OffsetX += (MaxWidth.Value - this.Width);
				else    // Center
					OffsetX += (MaxWidth.Value - this.Width) * 0.5;
			}

			if (MaxHeight.HasValue && this.valign.TryEvaluate(Session, out VerticalAlignment VAlignment) &&
				VAlignment != VerticalAlignment.Top)
			{
				if (VAlignment == VerticalAlignment.Bottom)
					OffsetY += (MaxHeight.Value - this.Height);
				else    // Center
					OffsetY += (MaxHeight.Value - this.Height) * 0.5;
			}
		}

		/// <summary>
		/// Calculates the span of the cell.
		/// </summary>
		/// <param name="Session">Current session.</param>
		/// <param name="ColSpan">Column span</param>
		/// <param name="RowSpan">Row span</param>
		public void CalcSpan(Variables Session, out int ColSpan, out int RowSpan)
		{
			if (this.colSpan.TryEvaluate(Session, out int Span))
				ColSpan = Span;
			else
				ColSpan = 1;

			if (this.rowSpan.TryEvaluate(Session, out Span))
				RowSpan = Span;
			else
				RowSpan = 1;
		}

	}
}
