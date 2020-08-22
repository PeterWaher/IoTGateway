using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A skew transform along the Y-axis.
	/// </summary>
	public class SkewY : PivotTrasformation
	{
		private FloatAttribute factor;

		/// <summary>
		/// A skew transform along the Y-axis.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SkewY(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "SkewY";

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

			this.factor?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new SkewY(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is SkewY Dest)
				Dest.factor = this.factor?.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			bool Relative = base.MeasureDimensions(State);

			if (!(this.factor is null) && this.factor.TryEvaluate(State.Session, out this.sy))
			{
				SKMatrix M = SKMatrix.CreateTranslation(this.xCoordinate, this.yCoordinate);
				M = M.PreConcat(SKMatrix.CreateSkew(0, this.sy));
				M = M.PreConcat(SKMatrix.CreateTranslation(-this.xCoordinate, -this.yCoordinate));

				this.TransformBoundingBox(M);
			}
			else
				this.sy = 0;

			return Relative;
		}

		private float sy;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Translate(this.xCoordinate, this.yCoordinate);
			State.Canvas.Skew(0, this.sy);
			State.Canvas.Translate(-this.xCoordinate, -this.yCoordinate);

			base.Draw(State);

			State.Canvas.SetMatrix(M);
		}

	}
}
