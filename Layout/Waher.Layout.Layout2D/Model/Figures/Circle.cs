using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A circle
	/// </summary>
	public class Circle : FigurePoint
	{
		private LengthAttribute radius;
		private float r;

		/// <summary>
		/// A circle
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Circle(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Circle";

		/// <summary>
		/// Radius
		/// </summary>
		public LengthAttribute Radius
		{
			get => this.radius;
			set => this.radius = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.radius = new LengthAttribute(Input, "radius");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.radius.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Circle(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Circle Dest)
				Dest.radius = this.radius.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.radius.TryEvaluate(State.Session, out Length R))
				this.r = State.GetDrawingSize(R, this, true);
			else
				this.defined = false;

			if (this.defined)
			{
				this.IncludePoint(this.xCoordinate - this.r, this.yCoordinate);
				this.IncludePoint(this.xCoordinate + this.r, this.yCoordinate);
				this.IncludePoint(this.xCoordinate, this.yCoordinate - this.r);
				this.IncludePoint(this.xCoordinate, this.yCoordinate + this.r);
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
					State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Fill);

				if (this.TryGetPen(State, out SKPaint Pen))
					State.Canvas.DrawCircle(this.xCoordinate, this.yCoordinate, this.r, Pen);
			}
		}
	}
}
