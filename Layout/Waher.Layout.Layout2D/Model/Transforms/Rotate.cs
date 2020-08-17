using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A rotation transform
	/// </summary>
	public class Rotate : PivotTrasformation
	{
		private FloatAttribute degrees;

		/// <summary>
		/// A rotation transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Rotate(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Rotate";

		/// <summary>
		/// Degrees
		/// </summary>
		public FloatAttribute Degrees
		{
			get => this.degrees;
			set => this.degrees = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.degrees = new FloatAttribute(Input, "degrees");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.degrees.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Rotate(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Rotate Dest)
				Dest.degrees = this.degrees.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.degrees.TryEvaluate(State.Session, out this.angle))
			{
				SKMatrix M = SKMatrix.CreateRotationDegrees(this.angle, this.xCoordinate, this.yCoordinate);
				this.TransformBoundingBox(M);
			}
			else
				this.angle = 0;
		}

		private float angle;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.RotateDegrees(this.angle, this.xCoordinate, this.yCoordinate);
			
			base.Draw(State);

			State.Canvas.SetMatrix(M);
		}
	}
}
