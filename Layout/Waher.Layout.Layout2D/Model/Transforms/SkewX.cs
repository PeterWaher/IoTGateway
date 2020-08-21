using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A skew transform along the X-axis.
	/// </summary>
	public class SkewX : PivotTrasformation
	{
		private FloatAttribute factor;

		/// <summary>
		/// A skew transform along the X-axis.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SkewX(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "SkewX";

		/// <summary>
		/// Factor
		/// </summary>
		public FloatAttribute FactorAttribute
		{
			get => this.factor;
			set => this.factor = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.factor = new FloatAttribute(Input, "factor");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.factor.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new SkewX(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is SkewX Dest)
				Dest.factor = this.factor.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			if (this.factor.TryEvaluate(State.Session, out this.sx))
			{
				SKMatrix M = SKMatrix.CreateTranslation(this.xCoordinate, this.yCoordinate);
				M = M.PreConcat(SKMatrix.CreateSkew(this.sx, 0));
				M = M.PreConcat(SKMatrix.CreateTranslation(-this.xCoordinate, -this.yCoordinate));

				this.TransformBoundingBox(M);
			}
			else
				this.sx = 0;
		}

		private float sx;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Translate(this.xCoordinate, this.yCoordinate);
			State.Canvas.Skew(this.sx, 0);
			State.Canvas.Translate(-this.xCoordinate, -this.yCoordinate);

			base.Draw(State);

			State.Canvas.SetMatrix(M);
		}

	}
}
