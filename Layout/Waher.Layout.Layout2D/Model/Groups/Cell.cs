using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Pens;
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
		/// Aligned along text base line.
		/// </summary>
		BaseLine,

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
		private StringAttribute border;

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
		/// Degrees
		/// </summary>
		public EnumAttribute<HorizontalAlignment> HorizontalAlignmentAttribute
		{
			get => this.halign;
			set => this.halign = value;
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public EnumAttribute<VerticalAlignment> VerticalAlignmentAttribute
		{
			get => this.valign;
			set => this.valign = value;
		}

		/// <summary>
		/// Column span
		/// </summary>
		public PositiveIntegerAttribute ColSpanAttribute
		{
			get => this.colSpan;
			set => this.colSpan = value;
		}

		/// <summary>
		/// Row span
		/// </summary>
		public PositiveIntegerAttribute RowSpanAttribute
		{
			get => this.rowSpan;
			set => this.rowSpan = value;
		}

		/// <summary>
		/// Border
		/// </summary>
		public StringAttribute BorderAttribute
		{
			get => this.border;
			set => this.border = value;
		}

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
			this.border = new StringAttribute(Input, "border");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.halign?.Export(Output);
			this.valign?.Export(Output);
			this.colSpan?.Export(Output);
			this.rowSpan?.Export(Output);
			this.border?.Export(Output);
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
				Dest.halign = this.halign?.CopyIfNotPreset();
				Dest.valign = this.valign?.CopyIfNotPreset();
				Dest.colSpan = this.colSpan?.CopyIfNotPreset();
				Dest.rowSpan = this.rowSpan?.CopyIfNotPreset();
				Dest.border = this.border?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Aligns a measured cell
		/// </summary>
		/// <param name="MaxWidth">Maximum width of area assigned to the cell</param>
		/// <param name="MaxHeight">Maximum height of area assigned to the cell</param>
		/// <param name="Session">Current session.</param>
		public void AlignedMeasuredCell(float? MaxWidth, float? MaxHeight, Variables Session)
		{
			if (MaxWidth.HasValue)
			{
				float? Width = this.Width;

				if (Width.HasValue &&
					!(this.halign is null) &&
					this.halign.TryEvaluate(Session, out HorizontalAlignment HAlignment) &&
					HAlignment != HorizontalAlignment.Left)
				{
					if (HAlignment == HorizontalAlignment.Right)
						this.dx = (MaxWidth.Value - Width.Value);
					else    // Center
						this.dx = (MaxWidth.Value - Width.Value) / 2;
				}

				this.Width = MaxWidth.Value;
			}

			if (MaxHeight.HasValue)
			{
				float? Height = this.Height;

				if (Height.HasValue &&
					!(this.valign is null) &&
					this.valign.TryEvaluate(Session, out VerticalAlignment VAlignment) &&
					VAlignment != VerticalAlignment.Top)
				{
					if (VAlignment == VerticalAlignment.Bottom || VAlignment == Groups.VerticalAlignment.BaseLine)
						this.dy = (MaxHeight.Value - Height.Value);
					else    // Center
						this.dy = (MaxHeight.Value - Height.Value) / 2;
				}

				this.Height = MaxHeight.Value;
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
			if (!(this.colSpan is null) && this.colSpan.TryEvaluate(Session, out int Span))
				ColSpan = Span;
			else
				ColSpan = 1;

			if (!(this.rowSpan is null) && this.rowSpan.TryEvaluate(Session, out Span))
				RowSpan = Span;
			else
				RowSpan = 1;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			SKRect? BoundingBox = this.BoundingRect;
			SKSize? PrevSize;

			if (BoundingBox.HasValue)
				PrevSize = State.SetAreaSize(BoundingBox.Value.Size);
			else
				PrevSize = null;
			
			bool Relative = base.MeasureDimensions(State);

			if (!(this.border is null) &&
				this.border.TryEvaluate(State.Session, out string RefId) &&
				this.Document.TryGetElement(RefId, out ILayoutElement Element) &&
				Element is Pen Pen)
			{
				this.borderPen = Pen;
			}

			this.dx = 0;
			this.dy = 0;

			if (PrevSize.HasValue)
				State.SetAreaSize(PrevSize.Value);

			return Relative;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			SKRect? BoundingBox = this.BoundingRect;
			SKSize? PrevSize;

			if (BoundingBox.HasValue)
				PrevSize = State.SetAreaSize(BoundingBox.Value.Size);
			else
				PrevSize = null;

			base.MeasurePositions(State);

			if (PrevSize.HasValue)
				State.SetAreaSize(PrevSize.Value);
		}

		private Pen borderPen;
		private float dx;
		private float dy;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (!(this.borderPen is null))
			{
				SKRect? Rect = this.BoundingRect;

				if (Rect.HasValue)
					State.Canvas.DrawRect(Rect.Value, this.borderPen.Paint);
			}

			if (this.dx == 0 && this.dy == 0)
				base.Draw(State);
			else
			{
				SKMatrix M = State.Canvas.TotalMatrix;
				State.Canvas.Translate(this.dx, this.dy);

				base.Draw(State);

				State.Canvas.SetMatrix(M);
			}
		}
	}
}
