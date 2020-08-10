using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A circle arc
	/// </summary>
	public class CircleArc : FigurePoint
	{
		private LengthAttribute radius;
		private FloatAttribute startDegrees;
		private FloatAttribute endDegrees;
		private BooleanAttribute clockwise;
		private float r;
		private float start;
		private float end;
		private bool clockDir;

		/// <summary>
		/// A circle arc
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public CircleArc(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "CircleArc";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.radius = new LengthAttribute(Input, "radius");
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

			this.radius.Export(Output);
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
			return new CircleArc(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is CircleArc Dest)
			{
				Dest.radius = this.radius.CopyIfNotPreset();
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

			if (this.radius.TryEvaluate(State.Session, out Length R))
				this.r = State.GetDrawingSize(R, this, true);
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

				this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, this.start);

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

						this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, a);
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

						this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, a);
					}
				}

				if (this.start != this.end)
					this.IncludePoint(this.xCoordinate, this.yCoordinate, this.r, this.r, this.end);
			}
		}
	}
}
