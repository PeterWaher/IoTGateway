using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
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
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.text = new StringAttribute(Input, "text");
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign");
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign");
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
				Dest.text = this.text?.CopyIfNotPreset();
				Dest.halign = this.halign?.CopyIfNotPreset();
				Dest.valign = this.valign?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			bool Relative = base.MeasureDimensions(State);

			if (this.halign is null || !this.halign.TryEvaluate(State.Session, out this.halignment))
				this.halignment = HorizontalAlignment.Left;

			if (this.valign is null || !this.valign.TryEvaluate(State.Session, out this.valignment))
				this.valignment = VerticalAlignment.Top;

			if (!(this.text is null) && this.text.TryEvaluate(State.Session, out this.textValue))
			{
				State.Text.MeasureText(this.textValue, ref this.bounds);

				this.Width = this.bounds.Width;
				this.Height = this.bounds.Height;
			}
			else
				this.defined = false;

			return Relative;
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

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.defined)
				State.Canvas.DrawText(this.textValue, this.xCoordinate, this.yCoordinate, State.Font, State.Text);
		
			base.Draw(State);
		}
	}
}
