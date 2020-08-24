using SkiaSharp;
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
		/// Width
		/// </summary>
		public LengthAttribute WidthAttribute
		{
			get => this.width;
			set => this.width = value;
		}

		/// <summary>
		/// Height
		/// </summary>
		public LengthAttribute HeightAttribute
		{
			get => this.height;
			set => this.height = value;
		}

		/// <summary>
		/// Maximum Width
		/// </summary>
		public LengthAttribute MaxWidthAttribute
		{
			get => this.maxWidth;
			set => this.maxWidth = value;
		}

		/// <summary>
		/// Maximum Height
		/// </summary>
		public LengthAttribute MaxHeightAttribute
		{
			get => this.maxHeight;
			set => this.maxHeight = value;
		}

		/// <summary>
		/// Minimum Width
		/// </summary>
		public LengthAttribute MinWidthAttribute
		{
			get => this.minWidth;
			set => this.minWidth = value;
		}

		/// <summary>
		/// Minimum Height
		/// </summary>
		public LengthAttribute MinHeightAttribute
		{
			get => this.minHeight;
			set => this.minHeight = value;
		}

		/// <summary>
		/// Keep aspect-ratio
		/// </summary>
		public BooleanAttribute KeepAspectRatioAttribute
		{
			get => this.keepAspectRatio;
			set => this.keepAspectRatio = value;
		}

		/// <summary>
		/// Overflow
		/// </summary>
		public EnumAttribute<Overflow> OverflowAttribute
		{
			get => this.overflow;
			set => this.overflow = value;
		}

		/// <summary>
		/// OnClick event script
		/// </summary>
		public ExpressionAttribute OnClickAttribute
		{
			get => this.onClick;
			set => this.onClick = value;
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

			this.width?.Export(Output);
			this.height?.Export(Output);
			this.maxWidth?.Export(Output);
			this.maxHeight?.Export(Output);
			this.minWidth?.Export(Output);
			this.minHeight?.Export(Output);
			this.keepAspectRatio?.Export(Output);
			this.overflow?.Export(Output);
			this.onClick?.Export(Output);
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
				Dest.width = this.width?.CopyIfNotPreset();
				Dest.height = this.height?.CopyIfNotPreset();
				Dest.maxWidth = this.maxWidth?.CopyIfNotPreset();
				Dest.maxHeight = this.maxHeight?.CopyIfNotPreset();
				Dest.minWidth = this.minWidth?.CopyIfNotPreset();
				Dest.minHeight = this.minHeight?.CopyIfNotPreset();
				Dest.keepAspectRatio = this.keepAspectRatio?.CopyIfNotPreset();
				Dest.overflow = this.overflow?.CopyIfNotPreset();
				Dest.onClick = this.onClick?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool DoMeasureDimensions(DrawingState State)
		{
			bool Relative = base.DoMeasureDimensions(State);
			float a;

			if (!(this.width is null) && this.width.TryEvaluate(State.Session, out Length L))
			{
				a = this.Width ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.Width = this.ExplicitWidth = a;
			}

			if (!(this.height is null) && this.height.TryEvaluate(State.Session, out L))
			{
				a = this.Height ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.Height = this.ExplicitHeight = a;
			}

			if (!(this.minWidth is null) && this.minWidth.TryEvaluate(State.Session, out L))
			{
				a = this.MinWidth ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.MinWidth = a;
			}

			if (!(this.maxWidth is null) && this.maxWidth.TryEvaluate(State.Session, out L))
			{
				a = this.MaxWidth ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.MaxWidth = a;
			}

			if (!(this.minHeight is null) && this.minHeight.TryEvaluate(State.Session, out L))
			{
				a = this.MinHeight ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.MinHeight = a;
			}

			if (!(this.maxHeight is null) && this.maxHeight.TryEvaluate(State.Session, out L))
			{
				a = this.MaxHeight ?? 0;
				State.CalcDrawingSize(L, ref a, true, ref Relative);
				this.MaxHeight = a;
			}

			return Relative;
		}

	}
}
