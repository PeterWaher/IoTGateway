using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// An ellipse arc
	/// </summary>
	public class EllipseArc : FigurePoint
	{
		private LengthAttribute radiusX;
		private LengthAttribute radiusY;
		private FloatAttribute startDegrees;
		private FloatAttribute endDegrees;
		private BooleanAttribute clockwise;
		private float rX;
		private float rY;
		private float start;
		private float end;
		private bool clockDir;

		/// <summary>
		/// An ellipse arc
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EllipseArc(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "EllipseArc";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.radiusX = new LengthAttribute(Input, "radiusX");
			this.radiusY = new LengthAttribute(Input, "radiusY");
			this.startDegrees = new FloatAttribute(Input, "startDegrees");
			this.endDegrees = new FloatAttribute(Input, "endDegrees");
			this.clockwise = new BooleanAttribute(Input, "clockwise");
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
			this.startDegrees.Export(Output);
			this.endDegrees.Export(Output);
			this.clockwise.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new EllipseArc(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is EllipseArc Dest)
			{
				Dest.radiusX = this.radiusX.CopyIfNotPreset();
				Dest.radiusY = this.radiusY.CopyIfNotPreset();
				Dest.startDegrees = this.startDegrees.CopyIfNotPreset();
				Dest.endDegrees = this.endDegrees.CopyIfNotPreset();
				Dest.clockwise = this.clockwise.CopyIfNotPreset();
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

			if (this.startDegrees.TryEvaluate(State.Session, out this.start))
				this.start = (float)Math.IEEERemainder(this.start, 360);
			else
				this.defined = false;

			if (this.endDegrees.TryEvaluate(State.Session, out this.end))
				this.end = (float)Math.IEEERemainder(this.end, 360);
			else
				this.defined = false;

			if (!this.clockwise.TryEvaluate(State.Session, out this.clockDir))
				this.defined = false;

			if (this.defined)
			{
				float a = this.start;
				float r = (float)Math.IEEERemainder(this.start, 90);
				bool First = true;

				this.IncludePoint(this.xCoordinate, this.yCoordinate, this.rX, this.rY, this.start);

				if (this.clockDir)
				{
					if (this.end < this.start)
						this.end += 360;

					while (a < this.end)
					{
						if (First)
						{
							a += 90 - r;
							First = false;
						}
						else
							a += 90;

						this.IncludePoint(this.xCoordinate, this.yCoordinate, this.rX, this.rY, a);
					}
				}
				else
				{
					if (this.end > this.start)
						this.end -= 360;

					while (a > this.end)
					{
						if (First)
						{
							a -= r;
							First = false;
						}
						else
							a -= 90;

						this.IncludePoint(this.xCoordinate, this.yCoordinate, this.rX, this.rY, a);
					}
				}

				if (this.start != this.end)
					this.IncludePoint(this.xCoordinate, this.yCoordinate, this.rX, this.rY, this.end);
			}
		}
	}
}
