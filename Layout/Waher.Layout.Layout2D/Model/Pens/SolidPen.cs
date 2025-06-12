﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Pens
{
	/// <summary>
	/// A solid pen
	/// </summary>
	public class SolidPen : Pen 
	{
		private ColorAttribute color;

		/// <summary>
		/// A solid pen
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SolidPen(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "SolidPen";

		/// <summary>
		/// Color
		/// </summary>
		public ColorAttribute ColorAttribute
		{
			get => this.color;
			set => this.color = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.color = new ColorAttribute(Input, "color", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.color?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new SolidPen(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is SolidPen Dest)
				Dest.color = this.color?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			if (this.paint is null)
			{
				EvaluationResult<SKColor> Color = await this.color.TryEvaluate(State.Session);
				if (Color.Ok)
				{
					this.paint = new SKPaint()
					{
						FilterQuality = SKFilterQuality.High,
						IsAntialias = true,
						Style = SKPaintStyle.Stroke,
						Color = Color.Result,
					};

					if (this.penWidth.HasValue)
						this.paint.StrokeWidth = this.penWidth.Value;

					if (this.penCap.HasValue)
						this.paint.StrokeCap = this.penCap.Value;

					if (this.penJoin.HasValue)
						this.paint.StrokeJoin = this.penJoin.Value;

					if (this.penMiter.HasValue)
						this.paint.StrokeMiter = this.penMiter.Value;

					this.defined = true;
				}
			}
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.color?.ExportState(Output);
		}

	}
}
