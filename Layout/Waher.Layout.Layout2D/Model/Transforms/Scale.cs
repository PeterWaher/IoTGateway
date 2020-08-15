using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A scale transform
	/// </summary>
	public class Scale : PivotTrasformation
	{
		private FloatAttribute scaleX;
		private FloatAttribute scaleY;

		/// <summary>
		/// A scale transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Scale(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Scale";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.scaleX = new FloatAttribute(Input, "scaleX");
			this.scaleY = new FloatAttribute(Input, "scaleY");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.scaleX.Export(Output);
			this.scaleY.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Scale(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Scale Dest)
			{
				Dest.scaleX = this.scaleX.CopyIfNotPreset();
				Dest.scaleY = this.scaleY.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (!this.scaleX.TryEvaluate(State.Session, out this.sx))
				this.sx = 1;

			if (!this.scaleY.TryEvaluate(State.Session, out this.sy))
				this.sy = 1;

			SKMatrix M = SKMatrix.CreateScale(this.sx, this.sy, this.xCoordinate, this.yCoordinate);
			this.TransformBoundingBox(M);
		}

		private float sx;
		private float sy;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Scale(this.sx, this.sy, this.xCoordinate, this.yCoordinate);

			base.Draw(State);

			State.Canvas.SetMatrix(M);
		}
	}
}
