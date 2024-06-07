using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Pens;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// Root node for two-dimensional layouts
	/// </summary>
	public class Layout2D : LayoutContainer
	{
		private StringAttribute font;
		private StringAttribute pen;
		private StringAttribute background;
		private ColorAttribute textColor;

		/// <summary>
		/// Root node for two-dimensional layouts
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Layout2D(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Layout2D";

		/// <summary>
		/// Font ID
		/// </summary>
		public StringAttribute FontIdAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Pen ID
		/// </summary>
		public StringAttribute PenIdAttribute
		{
			get => this.pen;
			set => this.pen = value;
		}

		/// <summary>
		/// Background ID
		/// </summary>
		public StringAttribute BackgroundIdAttribute
		{
			get => this.background;
			set => this.background = value;
		}

		/// <summary>
		/// Text Color
		/// </summary>
		public ColorAttribute TextColorAttribute
		{
			get => this.textColor;
			set => this.textColor = value;
		}

		/// <summary>
		/// Background Color
		/// </summary>
		public StringAttribute BackgroundColorAttribute
		{
			get => this.background;
			set => this.background = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.font = new StringAttribute(Input, "font", this.Document);
			this.pen = new StringAttribute(Input, "pen", this.Document);
			this.background = new StringAttribute(Input, "background", this.Document);
			this.textColor = new ColorAttribute(Input, "textColor", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.font?.Export(Output);
			this.pen?.Export(Output);
			this.background?.Export(Output);
			this.textColor?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Layout2D(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Layout2D Dest)
			{
				Dest.font = this.font?.CopyIfNotPreset(Destination.Document);
				Dest.pen = this.pen?.CopyIfNotPreset(Destination.Document);
				Dest.background = this.background?.CopyIfNotPreset(Destination.Document);
				Dest.textColor = this.textColor?.CopyIfNotPreset(Destination.Document);
			}
		}

		private Font fontDef = null;
		private Pen penDef = null;
		private Background backgroundDef = null;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;
			SKPaint PenBak = null;
			SKPaint BackgroundBak = null;

			EvaluationResult<string> RefId = await this.font.TryEvaluate(State.Session);
			if (RefId.Ok &&
				this.Document.TryGetElement(RefId.Result, out ILayoutElement Element) &&
				Element is Font Font)
			{
				this.fontDef = Font;

				await Font.MeasureDimensions(State);

				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			RefId = await this.pen.TryEvaluate(State.Session);
			if (RefId.Ok &&
				this.Document.TryGetElement(RefId.Result, out Element) &&
				Element is Pen Pen)
			{
				this.penDef = Pen;
				PenBak = State.ShapePen;
				State.ShapePen = Pen.Paint;
			}

			RefId = await this.background.TryEvaluate(State.Session);
			if (RefId.Ok &&
				this.Document.TryGetElement(RefId.Result, out Element) &&
				Element is Background Background)
			{
				this.backgroundDef = Background;
				BackgroundBak = State.ShapeFill;
				State.ShapeFill = Background.Paint;
			}

			await base.DoMeasureDimensions(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;

			if (!(PenBak is null))
				State.ShapePen = PenBak;

			if (!(BackgroundBak is null))
				State.ShapeFill = BackgroundBak;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;
			SKPaint PenBak = null;
			SKPaint BackgroundBak = null;

			if (!(this.fontDef is null))
			{
				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			if (!(this.penDef is null))
			{
				PenBak = State.ShapePen;
				State.ShapePen = this.penDef.Paint;
			}

			if (!(this.backgroundDef is null))
			{
				BackgroundBak = State.ShapeFill;
				State.ShapeFill = this.backgroundDef.Paint;
			}

			base.MeasurePositions(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;

			if (!(PenBak is null))
				State.ShapePen = PenBak;

			if (!(BackgroundBak is null))
				State.ShapeFill = BackgroundBak;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;
			SKPaint PenBak = null;
			SKPaint BackgroundBak = null;

			if (!(this.fontDef is null))
			{
				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			if (!(this.penDef is null))
			{
				PenBak = State.ShapePen;
				State.ShapePen = this.penDef.Paint;
			}

			if (!(this.backgroundDef is null))
			{
				BackgroundBak = State.ShapeFill;
				State.ShapeFill = this.backgroundDef.Paint;

				if (!(this.backgroundDef is SolidBackground))
					State.Canvas.DrawRect(0, 0, State.AreaWidth, State.AreaHeight, State.ShapeFill);
			}

			await base.Draw(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;

			if (!(PenBak is null))
				State.ShapePen = PenBak;

			if (!(BackgroundBak is null))
				State.ShapeFill = BackgroundBak;
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.font?.ExportState(Output);
			this.pen?.ExportState(Output);
			this.background?.ExportState(Output);
			this.textColor?.ExportState(Output);
		}
	}
}
