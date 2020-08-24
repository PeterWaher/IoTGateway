using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for distance elements.
	/// </summary>
	public abstract class Distance : LayoutElement
	{
		private LengthAttribute distance;

		/// <summary>
		/// Abstract base class for distance elements.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Distance(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public LengthAttribute DistanceAttribute
		{
			get => this.distance;
			set => this.distance = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.distance = new LengthAttribute(Input, "distance");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.distance?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Distance Dest)
				Dest.distance = this.distance?.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			bool Relative = base.MeasureDimensions(State);

			if (!(this.distance is null) && this.distance.TryEvaluate(State.Session, out Length L))
				State.CalcDrawingSize(L, ref this.dist, true, ref Relative);
			else
				this.defined = false;

			return Relative;
		}

		/// <summary>
		/// Measured distance
		/// </summary>
		protected float dist;

	}
}
