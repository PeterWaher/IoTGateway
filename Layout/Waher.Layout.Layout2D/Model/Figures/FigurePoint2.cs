using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures with two points.
	/// </summary>
	public abstract class FigurePoint2 : FigurePoint
	{
		private LengthAttribute x2;
		private LengthAttribute y2;
		private StringAttribute ref2;

		/// <summary>
		/// Abstract base class for figures with two points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public FigurePoint2(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// X-coordinate 2
		/// </summary>
		public LengthAttribute X2Attribute
		{
			get => this.x2;
			set => this.x2 = value;
		}

		/// <summary>
		/// Y-coordinate 2
		/// </summary>
		public LengthAttribute Y2Attribute
		{
			get => this.y2;
			set => this.y2 = value;
		}

		/// <summary>
		/// Reference 2
		/// </summary>
		public StringAttribute Reference2Attribute
		{
			get => this.ref2;
			set => this.ref2 = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.x2 = new LengthAttribute(Input, "x2");
			this.y2 = new LengthAttribute(Input, "y2");
			this.ref2 = new StringAttribute(Input, "ref2");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x2.Export(Output);
			this.y2.Export(Output);
			this.ref2.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is FigurePoint2 Dest)
			{
				Dest.x2 = this.x2.CopyIfNotPreset();
				Dest.y2 = this.y2.CopyIfNotPreset();
				Dest.ref2 = this.ref2.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			if (!this.IncludePoint(State, this.x2, this.y2, this.ref2, out this.xCoordinate2, out this.yCoordinate2))
			{
				float? dx = this.GetWidthEstimate(State);
				float? dy = this.GetHeightEstimate(State);

				if (dx.HasValue)
					this.xCoordinate2 = this.xCoordinate + dx.Value;
				else
					this.xCoordinate2 = this.xCoordinate + this.GetDefaultWidth(State);

				if (dy.HasValue)
					this.yCoordinate2 = this.yCoordinate + dy.Value;
				else
					this.yCoordinate2 = this.yCoordinate + this.GetDefaultHeight(State);

				this.IncludePoint(this.xCoordinate2, this.yCoordinate2);
			}
		}

		/// <summary>
		/// Default width of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Width</returns>
		public virtual float GetDefaultWidth(DrawingState State)
		{
			if (this.HasChildren && this.Width.HasValue)
				return this.Width.Value;
			else
				return State.GetDrawingSize(Length.HundredPercent, this, true);
		}

		/// <summary>
		/// Default height of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Height</returns>
		public virtual float GetDefaultHeight(DrawingState State)
		{
			if (this.HasChildren && this.Height.HasValue)
				return this.Height.Value;
			else
				return State.GetDrawingSize(Length.HundredPercent, this, false);
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate2;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate2;

	}
}
