using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// An ellipse
	/// </summary>
	public class Ellipse : FigurePoint
	{
		private LengthAttribute radiusX;
		private LengthAttribute radiusY;
		private float rX;
		private float rY;

		/// <summary>
		/// An ellipse
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Ellipse(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Ellipse";

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
			return new Ellipse(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Ellipse Dest)
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

			if (this.radiusX.TryEvaluate(State.Session, out Length R))
				this.rX = State.GetDrawingSize(R, this, true);
			else
				this.defined = false;

			if (this.radiusY.TryEvaluate(State.Session, out R))
				this.rY = State.GetDrawingSize(R, this, false);
			else
				this.defined = false;

			if (this.defined)
			{
				this.IncludePoint(this.xCoordinate - this.rX, this.yCoordinate);
				this.IncludePoint(this.xCoordinate + this.rX, this.yCoordinate);
				this.IncludePoint(this.xCoordinate, this.yCoordinate - this.rY);
				this.IncludePoint(this.xCoordinate, this.yCoordinate + this.rY);
			}
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (this.defined)
			{
				if (this.TryGetFill(State, out SKPaint Fill))
					State.Canvas.DrawOval(this.xCoordinate, this.yCoordinate, this.rX, this.rY, Fill);

				if (this.TryGetPen(State, out SKPaint Pen))
					State.Canvas.DrawOval(this.xCoordinate, this.yCoordinate, this.rX, this.rY, Pen);
			}
		}

	}
}
