using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Fonts
{
	/// <summary>
	/// Abstract base class for fonts.
	/// </summary>
	public class Font : LayoutElement
	{
		private StringAttribute name;
		private LengthAttribute size;
		private EnumAttribute<SKFontStyleWeight> weight;
		private EnumAttribute<SKFontStyleWidth> width;
		private EnumAttribute<SKFontStyleSlant> slant;
		private ColorAttribute color;

		/// <summary>
		/// Abstract base class for fonts.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Font(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Font";

		/// <summary>
		/// Name
		/// </summary>
		public StringAttribute NameAttribute
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Size
		/// </summary>
		public LengthAttribute SizeAttribute
		{
			get => this.size;
			set => this.size = value;
		}

		/// <summary>
		/// Weight
		/// </summary>
		public EnumAttribute<SKFontStyleWeight> WeightAttribute
		{
			get => this.weight;
			set => this.weight = value;
		}

		/// <summary>
		/// Width
		/// </summary>
		public EnumAttribute<SKFontStyleWidth> WidthAttribute
		{
			get => this.width;
			set => this.width = value;
		}

		/// <summary>
		/// Slant
		/// </summary>
		public EnumAttribute<SKFontStyleSlant> SlantAttribute
		{
			get => this.slant;
			set => this.slant = value;
		}

		/// <summary>
		/// Color
		/// </summary>
		public ColorAttribute ColorAttribute
		{
			get => this.color;
			set => this.color = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.name = new StringAttribute(Input, "name");
			this.size = new LengthAttribute(Input, "size");
			this.weight = new EnumAttribute<SKFontStyleWeight>(Input, "weight");
			this.width = new EnumAttribute<SKFontStyleWidth>(Input, "width");
			this.slant = new EnumAttribute<SKFontStyleSlant>(Input, "slant");
			this.color = new ColorAttribute(Input, "color");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.name?.Export(Output);
			this.size?.Export(Output);
			this.weight?.Export(Output);
			this.width?.Export(Output);
			this.slant?.Export(Output);
			this.color?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Font(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Font Dest)
			{
				Dest.name = this.name?.CopyIfNotPreset();
				Dest.size = this.size?.CopyIfNotPreset();
				Dest.weight = this.weight?.CopyIfNotPreset();
				Dest.width = this.width?.CopyIfNotPreset();
				Dest.slant = this.slant?.CopyIfNotPreset();
				Dest.color = this.color?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			bool Relative = base.MeasureDimensions(State);

			float Size;
			int Weight;
			int Width;

			if (this.name is null || !this.name.TryEvaluate(State.Session, out string Name))
				Name = State.Font.Typeface.FamilyName;

			Size = State.Font.Size;
			if (!(this.size is null) && this.size.TryEvaluate(State.Session, out Length Length))
				State.CalcDrawingSize(Length, ref Size, false, ref Relative);

			if (!(this.weight is null) && this.weight.TryEvaluate(State.Session, out SKFontStyleWeight W))
				Weight = (int)W;
			else
				Weight = State.Font.Typeface.FontWeight;

			if (!(this.width is null) && this.width.TryEvaluate(State.Session, out SKFontStyleWidth W2))
				Width = (int)W2;
			else
				Width = State.Font.Typeface.FontWidth;

			if (this.slant is null || !this.slant.TryEvaluate(State.Session, out SKFontStyleSlant Slant))
				Slant = State.Font.Typeface.FontSlant;

			if (this.color is null || !this.color.TryEvaluate(State.Session, out SKColor Color))
				Color = State.Text.Color;

			this.font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = (float)(Size * State.PixelsPerInch / 72),
				Typeface = SKTypeface.FromFamilyName(Name, Weight, Width, Slant)
			};

			this.text = new SKPaint()
			{
				FilterQuality = SKFilterQuality.High,
				HintingLevel = SKPaintHinting.Full,
				SubpixelText = true,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Color,
				Typeface = this.font.Typeface,
				TextSize = this.font.Size
			};

			return Relative;
		}

		private SKFont font;
		private SKPaint text;

		/// <summary>
		/// Measured Font
		/// </summary>
		public SKFont FontDef => this.font;

		/// <summary>
		/// Measured Text
		/// </summary>
		public SKPaint Text => this.text;
	}
}
