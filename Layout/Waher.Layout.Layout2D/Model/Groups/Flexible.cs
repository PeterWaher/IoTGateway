using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// How elements are ordered using flexible ordering.
	/// </summary>
	public enum FlexibleOrder
	{
		/// <summary>
		/// First horizontally, then vertically
		/// </summary>
		HorizontalVertical,

		/// <summary>
		/// First vertically, then horizontally
		/// </summary>
		VerticalHorizontal
	}

	/// <summary>
	/// Horizontal ordering
	/// </summary>
	public enum HorizontalDirection
	{
		/// <summary>
		/// From left to right
		/// </summary>
		LeftRight,

		/// <summary>
		/// From right to left
		/// </summary>
		RightLeft
	}

	/// <summary>
	/// Vertical ordering
	/// </summary>
	public enum VerticalDirection
	{
		/// <summary>
		/// From the top, downwards
		/// </summary>
		TopDown,

		/// <summary>
		/// From the bottom, upwards
		/// </summary>
		BottomUp
	}

	/// <summary>
	/// Ordering child elements flexibly.
	/// </summary>
	public class Flexible : SpatialDistribution
	{
		private EnumAttribute<FlexibleOrder> order;
		private EnumAttribute<HorizontalDirection> horizontalDirection;
		private EnumAttribute<VerticalDirection> verticalDirection;
		private EnumAttribute<HorizontalAlignment> halign;
		private EnumAttribute<VerticalAlignment> valign;

		/// <summary>
		/// Ordering child elements flexibly.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Flexible(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Flexible";

		/// <summary>
		/// Order
		/// </summary>
		public EnumAttribute<FlexibleOrder> OrderAttribute
		{
			get => this.order;
			set => this.order = value;
		}

		/// <summary>
		/// Horizontal Direction
		/// </summary>
		public EnumAttribute<HorizontalDirection> HorizontalDirectionAttribute
		{
			get => this.horizontalDirection;
			set => this.horizontalDirection = value;
		}

		/// <summary>
		/// Vertical Direction
		/// </summary>
		public EnumAttribute<VerticalDirection> VerticalDirectionAttribute
		{
			get => this.verticalDirection;
			set => this.verticalDirection = value;
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public EnumAttribute<HorizontalAlignment> HorizontalAlignmentAttribute
		{
			get => this.halign;
			set => this.halign = value;
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public EnumAttribute<VerticalAlignment> VerticalAlignmentAttribute
		{
			get => this.valign;
			set => this.valign = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.order = new EnumAttribute<FlexibleOrder>(Input, "order", this.Document);
			this.horizontalDirection = new EnumAttribute<HorizontalDirection>(Input, "horizontalDirection", this.Document);
			this.verticalDirection = new EnumAttribute<VerticalDirection>(Input, "verticalDirection", this.Document);
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign", this.Document);
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.order?.Export(Output);
			this.horizontalDirection?.Export(Output);
			this.verticalDirection?.Export(Output);
			this.halign?.Export(Output);
			this.valign?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Flexible(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Flexible Dest)
			{
				Dest.order = this.order?.CopyIfNotPreset(Destination.Document);
				Dest.horizontalDirection = this.horizontalDirection?.CopyIfNotPreset(Destination.Document);
				Dest.verticalDirection = this.verticalDirection?.CopyIfNotPreset(Destination.Document);
				Dest.halign = this.halign?.CopyIfNotPreset(Destination.Document);
				Dest.valign = this.valign?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Gets a cell layout object that will be responsible for laying out cells.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Cell layout object.</returns>
		public override async Task<ICellLayout> GetCellLayout(DrawingState State)
		{
			FlexibleOrder Order = await this.order.Evaluate(State.Session, FlexibleOrder.HorizontalVertical);
			HorizontalDirection HorizontalDirection = await this.horizontalDirection.Evaluate(State.Session, HorizontalDirection.LeftRight);
			VerticalDirection VerticalDirection = await this.verticalDirection.Evaluate(State.Session, VerticalDirection.TopDown);

			switch (Order)
			{
				case FlexibleOrder.HorizontalVertical:
				default:
					float Size = this.ExplicitWidth ?? this.Parent?.InnerWidth ?? State.AreaWidth;
					HorizontalAlignment HAlignment = await this.halign.Evaluate(State.Session,
						HorizontalDirection == HorizontalDirection.LeftRight ? HorizontalAlignment.Left : HorizontalAlignment.Right);

					return new FlexibleHorizontalCells(State.Session, Size, HorizontalDirection, VerticalDirection, HAlignment);

				case FlexibleOrder.VerticalHorizontal:
					Size = this.ExplicitHeight ?? this.Parent?.InnerHeight ?? State.AreaHeight;
					VerticalAlignment VAlignment = await this.valign.Evaluate(State.Session,
						VerticalDirection == VerticalDirection.TopDown ? VerticalAlignment.Top : VerticalAlignment.Bottom);

					return new FlexibleVerticalCells(State.Session, Size, HorizontalDirection, VerticalDirection, VAlignment);
			}
		}

	}
}
