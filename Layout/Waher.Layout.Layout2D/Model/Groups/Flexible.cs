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
	public class Flexible : LayoutContainer
	{
		private EnumAttribute<FlexibleOrder> order;
		private EnumAttribute<HorizontalDirection> horizontalDirection;
		private EnumAttribute<VerticalDirection> verticalDirection;

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
	}
}
