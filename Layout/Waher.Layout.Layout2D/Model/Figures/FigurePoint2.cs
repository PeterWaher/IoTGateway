using System.Threading.Tasks;
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
		public override Task FromXml(XmlElement Input)
		{
			this.x2 = new LengthAttribute(Input, "x2", this.Document);
			this.y2 = new LengthAttribute(Input, "y2", this.Document);
			this.ref2 = new StringAttribute(Input, "ref2", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x2?.Export(Output);
			this.y2?.Export(Output);
			this.ref2?.Export(Output);
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
				Dest.x2 = this.x2?.CopyIfNotPreset(Destination.Document);
				Dest.y2 = this.y2?.CopyIfNotPreset(Destination.Document);
				Dest.ref2 = this.ref2?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			CalculatedPoint P = await this.IncludePoint(State, this.x2, this.y2, this.@ref2, this.xCoordinate2, this.yCoordinate2);
			if (P.Ok)
			{
				this.xCoordinate2 = P.X;
				this.yCoordinate2 = P.Y;
			}
			else
			{
				float? Value = this.ExplicitWidth;
				float? Temp;

				if (!Value.HasValue)
				{
					Value = this.PotentialWidth;
					if ((Temp = this.Width).HasValue && (!Value.HasValue || Temp.Value > Value.Value))
						Value = Temp.Value;
				}

				if (Value.HasValue)
				{
					float X = this.xCoordinate + Value.Value;
					if (X != this.xCoordinate2)
					{
						State.MeasureRelative = true;
						this.xCoordinate2 = X;
					}
				}

				Value = this.ExplicitHeight;
				if (!Value.HasValue)
				{
					Value = this.PotentialHeight;
					if ((Temp = this.Height).HasValue && (!Value.HasValue || Temp.Value > Value.Value))
						Value = Temp.Value;
				}

				if (Value.HasValue)
				{
					float Y = this.yCoordinate + Value.Value;
					if (Y != this.yCoordinate2)
					{
						State.MeasureRelative = true;
						this.yCoordinate2 = Y;
					}
				}

				this.IncludePoint(this.xCoordinate2, this.yCoordinate2);
			}
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate2;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate2;

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.x2?.ExportState(Output);
			this.y2?.ExportState(Output);
			this.ref2?.ExportState(Output);
		}

	}
}
