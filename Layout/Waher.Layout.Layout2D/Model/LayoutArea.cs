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
		private ExpressionAttribute onClick;

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
			this.onClick = new ExpressionAttribute(Input, "onClick");
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

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is LayoutArea Dest)
			{
				Dest.width = this.width.CopyIfNotPreset();
				Dest.height = this.height.CopyIfNotPreset();
				Dest.maxWidth = this.maxWidth.CopyIfNotPreset();
				Dest.maxHeight = this.maxHeight.CopyIfNotPreset();
				Dest.minWidth = this.minWidth.CopyIfNotPreset();
				Dest.minHeight = this.minHeight.CopyIfNotPreset();
				Dest.keepAspectRatio = this.keepAspectRatio.CopyIfNotPreset();
				Dest.overflow = this.overflow.CopyIfNotPreset();
				Dest.onClick = this.onClick.CopyIfNotPreset();
			}
		}

		private float? GetSize(DrawingState State, bool Horizontal, params LengthAttribute[] Sizes)
		{
			foreach (LengthAttribute Attr in Sizes)
			{
				if (!Attr.TryEvaluate(State.Session, out Length L))
					continue;

				return State.GetDrawingSize(L, this, Horizontal);
			}

			return null;
		}

		/// <summary>
		/// Gets the maximum width of the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Maximum width, if defined.</returns>
		public float? GetMaxWidth(DrawingState State)
		{
			return this.GetSize(State, true, this.maxWidth, this.width);
		}

		/// <summary>
		/// Gets the minimum width of the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Maximum width, if defined.</returns>
		public float? GetMinWidth(DrawingState State)
		{
			return this.GetSize(State, true, this.minWidth, this.width);
		}

		/// <summary>
		/// Gets the maximum height of the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Maximum height, if defined.</returns>
		public float? GetMaxHeight(DrawingState State)
		{
			return this.GetSize(State, false, this.maxHeight, this.height);
		}

		/// <summary>
		/// Gets the minimum height of the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Maximum height, if defined.</returns>
		public float? GetMinHeight(DrawingState State)
		{
			return this.GetSize(State, false, this.minHeight, this.height);
		}

		/// <summary>
		/// Gets a width estimate.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Width estimate.</returns>
		public float? GetWidthEstimate(DrawingState State)
		{
			return this.GetSize(State, true, this.width, this.maxWidth, this.minWidth);
		}

		/// <summary>
		/// Gets a height estimate.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Height estimate.</returns>
		public float? GetHeightEstimate(DrawingState State)
		{
			return this.GetSize(State, false, this.height, this.minHeight, this.maxHeight);
		}

	}
}
