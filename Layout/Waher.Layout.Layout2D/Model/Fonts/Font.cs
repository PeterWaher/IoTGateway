﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Fonts
{
	/// <summary>
	/// Abstract base class for fonts.
	/// </summary>
	public class Font : LayoutElement
	{
		/// <summary>
		/// Line height by default is 40% larger than height of text.
		/// </summary>
		public const float DefaultLineHeightRelative = 1.4f;

		private StringAttribute name;
		private LengthAttribute size;
		private LengthAttribute lineHeight;
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
		/// Line Height
		/// </summary>
		public LengthAttribute LineHeightAttribute
		{
			get => this.lineHeight;
			set => this.lineHeight = value;
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
		public override Task FromXml(XmlElement Input)
		{
			this.name = new StringAttribute(Input, "name", this.Document);
			this.size = new LengthAttribute(Input, "size", this.Document);
			this.lineHeight = new LengthAttribute(Input, "lineHeight", this.Document);
			this.weight = new EnumAttribute<SKFontStyleWeight>(Input, "weight", this.Document);
			this.width = new EnumAttribute<SKFontStyleWidth>(Input, "width", this.Document);
			this.slant = new EnumAttribute<SKFontStyleSlant>(Input, "slant", this.Document);
			this.color = new ColorAttribute(Input, "color", this.Document);

			return base.FromXml(Input);
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
			this.lineHeight?.Export(Output);
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
				Dest.name = this.name?.CopyIfNotPreset(Destination.Document);
				Dest.size = this.size?.CopyIfNotPreset(Destination.Document);
				Dest.lineHeight = this.lineHeight?.CopyIfNotPreset(Destination.Document);
				Dest.weight = this.weight?.CopyIfNotPreset(Destination.Document);
				Dest.width = this.width?.CopyIfNotPreset(Destination.Document);
				Dest.slant = this.slant?.CopyIfNotPreset(Destination.Document);
				Dest.color = this.color?.CopyIfNotPreset(Destination.Document);
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

			float Size;
			int Weight;
			int Width;

			Size = State.Font.Size;
			EvaluationResult<Length> SizeLength = await this.size.TryEvaluate(State.Session);
			if (SizeLength.Ok)
				State.CalcDrawingSize(SizeLength.Result, ref Size, false, (ILayoutElement)this);

			this.lineHeightValue = Size * DefaultLineHeightRelative;
			EvaluationResult<Length> LineHeightLength = await this.lineHeight.TryEvaluate(State.Session);
			if (LineHeightLength.Ok)
			{
				State.CalcDrawingSize(LineHeightLength.Result, ref this.lineHeightValue, false, (ILayoutElement)this);
				this.lineHeightExplicit = true;
			}
			else
				this.lineHeightExplicit = false;

			this.lineHeightValue = this.lineHeightValue * State.PixelsPerInch / 72;

			EvaluationResult<SKFontStyleWeight> WeightValue = await this.weight.TryEvaluate(State.Session);
			if (WeightValue.Ok)
				Weight = (int)WeightValue.Result;
			else
				Weight = State.Font.Typeface.FontWeight;

			EvaluationResult<SKFontStyleWidth> WidthValue = await this.width.TryEvaluate(State.Session);
			if (WidthValue.Ok)
				Width = (int)WidthValue.Result;
			else
				Width = State.Font.Typeface.FontWidth;

			string Name = await this.name.Evaluate(State.Session, State.Font.Typeface.FamilyName);
			SKFontStyleSlant Slant = await this.slant.Evaluate(State.Session, State.Font.Typeface.FontSlant);
			SKColor Color = await this.color.Evaluate(State.Session, State.Text.Color);

			this.font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = Size * State.PixelsPerInch / 72,
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
		}

		private SKFont font;
		private SKPaint text;
		private float lineHeightValue;
		private bool lineHeightExplicit;

		/// <summary>
		/// Measured Font
		/// </summary>
		public SKFont FontDef => this.font;

		/// <summary>
		/// Measured Text
		/// </summary>
		public SKPaint Text => this.text;

		/// <summary>
		/// Line height.
		/// </summary>
		public float LineHeight => this.lineHeightValue;

		/// <summary>
		/// If line-height is explicitly defined
		/// </summary>
		public bool LineHeightExplicit => this.lineHeightExplicit;

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.name?.ExportState(Output);
			this.size?.ExportState(Output);
			this.lineHeight?.ExportState(Output);
			this.weight?.ExportState(Output);
			this.width?.ExportState(Output);
			this.slant?.ExportState(Output);
			this.color?.ExportState(Output);
		}
	}
}
