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

			this.name.Export(Output);
			this.size.Export(Output);
			this.weight.Export(Output);
			this.width.Export(Output);
			this.slant.Export(Output);
			this.color.Export(Output);
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
				Dest.name = this.name.CopyIfNotPreset();
				Dest.size = this.size.CopyIfNotPreset();
				Dest.weight = this.weight.CopyIfNotPreset();
				Dest.width = this.width.CopyIfNotPreset();
				Dest.slant = this.slant.CopyIfNotPreset();
				Dest.color = this.color.CopyIfNotPreset();
			}
		}
	}
}
