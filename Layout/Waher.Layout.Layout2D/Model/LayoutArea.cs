using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Overflow handling.
	/// </summary>
	public enum Overflow
	{
		/// <summary>
		/// Clip any content outside of the area.
		/// </summary>
		Clip,

		/// <summary>
		/// Ignore overflow
		/// </summary>
		Ignore
	}

	/// <summary>
	/// Abstract base class for layout elements with an implicit area.
	/// </summary>
	public abstract class LayoutArea : LayoutElement
	{
		private LengthAttribute width;
		private LengthAttribute height;
		private LengthAttribute maxWidth;
		private LengthAttribute maxHeight;
		private LengthAttribute minWidth;
		private LengthAttribute minHeight;
		private BooleanAttribute keepAspectRatio;
		private EnumAttribute<Overflow> overflow;
		private EventAttribute onClick;

		/// <summary>
		/// Abstract base class for layout elements with an implicit area.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LayoutArea(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.width = new LengthAttribute(Input, "width");
			this.height = new LengthAttribute(Input, "height");
			this.maxWidth = new LengthAttribute(Input, "maxWidth");
			this.maxHeight = new LengthAttribute(Input, "maxHeight");
			this.minWidth = new LengthAttribute(Input, "minWidth");
			this.minHeight = new LengthAttribute(Input, "minHeight");
			this.keepAspectRatio = new BooleanAttribute(Input, "keepAspectRatio");
			this.overflow = new EnumAttribute<Overflow>(Input, "overflow");
			this.onClick = new EventAttribute(Input, "onClick");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.width.Export(Output);
			this.height.Export(Output);
			this.maxWidth.Export(Output);
			this.maxHeight.Export(Output);
			this.minWidth.Export(Output);
			this.minHeight.Export(Output);
			this.keepAspectRatio.Export(Output);
			this.overflow.Export(Output);
			this.onClick.Export(Output);
		}
	}
}
