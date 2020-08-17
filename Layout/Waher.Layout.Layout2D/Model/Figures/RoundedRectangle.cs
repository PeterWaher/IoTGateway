using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A rounded rectangle
	/// </summary>
	public class RoundedRectangle : Rectangle
	{
		private LengthAttribute radiusX;
		private LengthAttribute radiusY;

		/// <summary>
		/// A rounded rectangle
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public RoundedRectangle(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "RoundedRectangle";

		/// <summary>
		/// Radius X
		/// </summary>
		public LengthAttribute RadiusX
		{
			get => this.radiusX;
			set => this.radiusX = value;
		}

		/// <summary>
		/// Radius Y
		/// </summary>
		public LengthAttribute RadiusY
		{
			get => this.radiusY;
			set => this.radiusY = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.radiusX = new LengthAttribute(Input, "radiusX");
			this.radiusY = new LengthAttribute(Input, "radiusY");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.radiusX.Export(Output);
			this.radiusY.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new RoundedRectangle(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is RoundedRectangle Dest)
			{
				Dest.radiusX = this.radiusX.CopyIfNotPreset();
				Dest.radiusY = this.radiusY.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.radiusX.TryEvaluate(State.Session, out Length L))
				this.rx = State.GetDrawingSize(L, this, true);
			else
				this.rx = 0;

			if (this.radiusY.TryEvaluate(State.Session, out L))
				this.ry = State.GetDrawingSize(L, this, false);
			else
				this.ry = 0;
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float rx;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float ry;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.defined)
			{
				this.defined = false;
				base.Draw(State);
				this.defined = true;

				if (this.TryGetFill(State, out SKPaint Fill))
				{
					State.Canvas.DrawRoundRect(this.xCoordinate, this.yCoordinate,
						this.xCoordinate2 - this.xCoordinate,
						this.yCoordinate2 - this.yCoordinate,
						this.rx, this.ry, Fill);
				}

				if (this.TryGetPen(State, out SKPaint Pen))
				{
					State.Canvas.DrawRoundRect(this.xCoordinate, this.yCoordinate,
						this.xCoordinate2 - this.xCoordinate,
						this.yCoordinate2 - this.yCoordinate,
						this.rx, this.ry, Pen);
				}
			}
			else
				base.Draw(State);
		}
	}
}
