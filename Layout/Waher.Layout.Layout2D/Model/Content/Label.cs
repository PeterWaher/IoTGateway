using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Groups;

namespace Waher.Layout.Layout2D.Model.Content
{
	/// <summary>
	/// Represents an unformatted text label.
	/// </summary>
	public class Label : Point
	{
		private StringAttribute text;
		private EnumAttribute<HorizontalAlignment> halign;
		private EnumAttribute<VerticalAlignment> valign;
		private StringAttribute font;

		/// <summary>
		/// Represents an unformatted text label.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Label(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Label";

		/// <summary>
		/// Text
		/// </summary>
		public StringAttribute TextAttribute
		{
			get => this.text;
			set => this.text = value;
		}

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
		/// Font
		/// </summary>
		public StringAttribute FontAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.text = new StringAttribute(Input, "text", this.Document);
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign", this.Document);
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign", this.Document);
			this.font = new StringAttribute(Input, "font", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.text?.Export(Output);
			this.halign?.Export(Output);
			this.valign?.Export(Output);
			this.font?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Label(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Label Dest)
			{
				Dest.text = this.text?.CopyIfNotPreset(Destination.Document);
				Dest.halign = this.halign?.CopyIfNotPreset(Destination.Document);
				Dest.valign = this.valign?.CopyIfNotPreset(Destination.Document);
				Dest.font = this.font?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			this.halignment = await this.halign.Evaluate(State.Session, HorizontalAlignment.Left);
			this.valignment = await this.valign.Evaluate(State.Session, VerticalAlignment.Top);

			if (!(this.font is null) && this.fontRef is null)
			{
				EvaluationResult<string> FontId = await this.font.TryEvaluate(State.Session);

				if (FontId.Ok && this.Document.TryGetElement(FontId.Result, out ILayoutElement E) && E is Font Font)
					this.fontRef = Font;
			}

			EvaluationResult<string> Text = await this.text.TryEvaluate(State.Session);
			if (Text.Ok)
			{
				this.textValue = Text.Result;

				SKPaint Paint = this.fontRef?.Text ?? State.Text;

				Paint.MeasureText(this.textValue, ref this.bounds);

				this.Width = this.bounds.Width;
				this.Height = this.bounds.Height;
			}
			else
				this.defined = false;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			base.MeasurePositions(State);

			if (this.defined)
			{
				switch (this.halignment)
				{
					case HorizontalAlignment.Left:
					default:
						this.Left = this.xCoordinate + this.bounds.Left;
						break;

					case HorizontalAlignment.Center:
						this.xCoordinate -= this.bounds.Width / 2;
						this.Left = this.xCoordinate;
						break;

					case HorizontalAlignment.Right:
						this.xCoordinate -= this.bounds.Width;
						this.Left = this.xCoordinate;
						break;
				}

				switch (this.valignment)
				{
					case VerticalAlignment.Top:
					default:
						this.yCoordinate -= this.bounds.Top;
						this.Top = this.yCoordinate;
						break;

					case VerticalAlignment.Center:
						this.yCoordinate -= this.bounds.Top;
						this.yCoordinate -= this.bounds.Height / 2;
						this.Top = this.yCoordinate + this.bounds.Top + this.bounds.Height / 2;
						break;

					case VerticalAlignment.BaseLine:
						this.Top = this.yCoordinate + this.bounds.Top;
						break;

					case VerticalAlignment.Bottom:
						this.yCoordinate -= this.bounds.Top;
						this.yCoordinate -= this.bounds.Height;
						this.Top = this.yCoordinate + this.bounds.Top;
						break;
				}
			}
		}

		private string textValue;
		private HorizontalAlignment halignment;
		private VerticalAlignment valignment;
		private SKRect bounds = new SKRect();
		private Font fontRef = null;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
			{
				State.Canvas.DrawText(this.textValue, this.xCoordinate, this.yCoordinate, 
					this.fontRef?.FontDef ?? State.Font, this.fontRef?.Text ?? State.Text);
			}

			await base.Draw(State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.text?.ExportState(Output);
			this.halign?.ExportState(Output);
			this.valign?.ExportState(Output);
			this.font?.ExportState(Output);
		}
	}
}
