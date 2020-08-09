using System;
using System.Collections.Generic;
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
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.order = new EnumAttribute<FlexibleOrder>(Input, "order");
			this.horizontalDirection = new EnumAttribute<HorizontalDirection>(Input, "horizontalDirection");
			this.verticalDirection = new EnumAttribute<VerticalDirection>(Input, "verticalDirection");
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign");
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.order.Export(Output);
			this.horizontalDirection.Export(Output);
			this.verticalDirection.Export(Output);
			this.halign.Export(Output);
			this.valign.Export(Output);
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
				Dest.order = this.order.CopyIfNotPreset();
				Dest.horizontalDirection = this.horizontalDirection.CopyIfNotPreset();
				Dest.verticalDirection = this.verticalDirection.CopyIfNotPreset();
				Dest.halign = this.halign.CopyIfNotPreset();
				Dest.valign = this.valign.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Gets a cell layout object that will be responsible for laying out cells.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Cell layout object.</returns>
		public override ICellLayout GetCellLayout(DrawingState State)
		{
			if (!this.order.TryEvaluate(State.Session, out FlexibleOrder Order))
				Order = FlexibleOrder.HorizontalVertical;

			if (!this.horizontalDirection.TryEvaluate(State.Session, out HorizontalDirection HorizontalDirection))
				HorizontalDirection = HorizontalDirection.LeftRight;

			if (!this.verticalDirection.TryEvaluate(State.Session, out VerticalDirection VerticalDirection))
				VerticalDirection = VerticalDirection.TopDown;

			switch (Order)
			{
				case FlexibleOrder.HorizontalVertical:
				default:
					double? Size = this.GetWidthEstimate(State);
					if (Size.HasValue)
					{
						if (!this.halign.TryEvaluate(State.Session, out HorizontalAlignment HorizontalAlignment))
						{
							if (HorizontalDirection == HorizontalDirection.LeftRight)
								HorizontalAlignment = HorizontalAlignment.Left;
							else
								HorizontalAlignment = HorizontalAlignment.Right;
						}

						return new FlexibleHorizontalCells(State.Session, Size.Value,
							HorizontalDirection, VerticalDirection, HorizontalAlignment);
					}
					else
						return new HorizontalCells(State.Session);

				case FlexibleOrder.VerticalHorizontal:
					Size = this.GetHeightEstimate(State);
					if (Size.HasValue)
					{
						if (!this.valign.TryEvaluate(State.Session, out VerticalAlignment VerticalAlignment))
						{
							if (VerticalDirection == VerticalDirection.TopDown)
								VerticalAlignment = VerticalAlignment.Top;
							else
								VerticalAlignment = VerticalAlignment.Bottom;
						}

						return new FlexibleVerticalCells(State.Session, Size.Value,
							HorizontalDirection, VerticalDirection, VerticalAlignment);
					}
					else
						return new VerticalCells(State.Session);
			}
		}

	}
}
