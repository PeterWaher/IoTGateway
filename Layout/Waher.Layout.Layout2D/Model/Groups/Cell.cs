﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
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
		private float? innerWidth;
		private float? innerHeight;

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
		public override Task FromXml(XmlElement Input)
		{
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign", this.Document);
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign", this.Document);
			this.colSpan = new PositiveIntegerAttribute(Input, "colSpan", this.Document);
			this.rowSpan = new PositiveIntegerAttribute(Input, "rowSpan", this.Document);
			this.border = new StringAttribute(Input, "border", this.Document);

			return base.FromXml(Input);
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
				Dest.halign = this.halign?.CopyIfNotPreset(Destination.Document);
				Dest.valign = this.valign?.CopyIfNotPreset(Destination.Document);
				Dest.colSpan = this.colSpan?.CopyIfNotPreset(Destination.Document);
				Dest.rowSpan = this.rowSpan?.CopyIfNotPreset(Destination.Document);
				Dest.border = this.border?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Aligns a measured cell
		/// </summary>
		/// <param name="MaxWidth">Maximum width of area assigned to the cell</param>
		/// <param name="MaxHeight">Maximum height of area assigned to the cell</param>
		/// <param name="Session">Current session.</param>
		/// <param name="SetPosition">If position of inner content is to be set..</param>
		public async Task Distribute(float? MaxWidth, float? MaxHeight, Variables Session, bool SetPosition)
		{
			if (MaxWidth.HasValue)
			{
				this.innerWidth = MaxWidth;

				if (SetPosition)
				{
					float? Width = this.Width;

					if (Width.HasValue)
					{
						EvaluationResult<HorizontalAlignment> HAlignment = await this.halign.TryEvaluate(Session);
						if (HAlignment.Ok && HAlignment.Result != HorizontalAlignment.Left)
						{
							if (HAlignment.Result == HorizontalAlignment.Right)
								this.dx = (MaxWidth.Value - Width.Value);
							else    // Center
								this.dx = (MaxWidth.Value - Width.Value) / 2;
						}
					}

					this.Width = MaxWidth.Value;
				}
			}

			if (MaxHeight.HasValue)
			{
				this.innerHeight = MaxHeight;

				if (SetPosition)
				{
					float? Height = this.Height;

					if (Height.HasValue)
					{
						EvaluationResult<VerticalAlignment> VAlignment = await this.valign.TryEvaluate(Session);
						if (VAlignment.Ok && VAlignment.Result != VerticalAlignment.Top)
						{
							if (VAlignment.Result == VerticalAlignment.Bottom ||
								VAlignment.Result == Groups.VerticalAlignment.BaseLine)
							{
								this.dy = (MaxHeight.Value - Height.Value);
							}
							else    // Center
								this.dy = (MaxHeight.Value - Height.Value) / 2;
						}
					}

					this.Height = MaxHeight.Value;
				}
			}
		}

		/// <summary>
		/// Inner Width of element
		/// </summary>
		public override float? InnerWidth => this.innerWidth;

		/// <summary>
		/// Inner Height of element
		/// </summary>
		public override float? InnerHeight => this.innerHeight;

		/// <summary>
		/// Calculates the span of the cell.
		/// </summary>
		/// <param name="Session">Current session.</param>
		/// <returns>Cell span</returns>
		public async Task<CellSpan> CalcSpan(Variables Session)
		{
			CellSpan Result;

			Result.ColSpan = await this.colSpan.Evaluate(Session, 1);
			Result.RowSpan = await this.rowSpan.Evaluate(Session, 1);

			return Result;
		}

		/// <summary>
		/// Potential Width of element
		/// </summary>
		public override float? PotentialWidth => this.innerWidth;

		/// <summary>
		/// Potential Height of element
		/// </summary>
		public override float? PotentialHeight => this.innerHeight;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			SKRect? BoundingBox = this.BoundingRect;
			SKSize? PrevSize;

			if (BoundingBox.HasValue)
				PrevSize = State.SetAreaSize(BoundingBox.Value.Size);
			else
				PrevSize = null;

			await base.DoMeasureDimensions(State);

			EvaluationResult<string> RefId = await this.border.TryEvaluate(State.Session);
			if (RefId.Ok  &&
				this.Document.TryGetElement(RefId.Result, out ILayoutElement Element) &&
				Element is Pen Pen)
			{
				this.borderPen = Pen;
			}

			this.dx = 0;
			this.dy = 0;

			if (PrevSize.HasValue)
				State.SetAreaSize(PrevSize.Value);
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
		public override async Task Draw(DrawingState State)
		{
			if (!(this.borderPen is null))
			{
				SKRect? Rect = this.BoundingRect;

				if (Rect.HasValue)
					State.Canvas.DrawRect(Rect.Value, this.borderPen.Paint);
			}

			if (this.dx == 0 && this.dy == 0)
				await base.Draw(State);
			else
			{
				SKMatrix M = State.Canvas.TotalMatrix;
				State.Canvas.Translate(this.dx, this.dy);

				await base.Draw(State);

				State.Canvas.SetMatrix(M);
			}
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.halign?.ExportState(Output);
			this.valign?.ExportState(Output);
			this.colSpan?.ExportState(Output);
			this.rowSpan?.ExportState(Output);
			this.border?.ExportState(Output);
		}
	}
}
