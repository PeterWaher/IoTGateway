using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Filters
{
	/// <summary>
	/// Blur image filter
	/// </summary>
	public class Blur : LayoutContainer
	{
		private FloatAttribute sigmaX;
		private FloatAttribute sigmaY;
		private EnumAttribute<SKShaderTileMode> tileMode;

		/// <summary>
		/// Blur image filter
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Blur(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Blur";

		/// <summary>
		/// Sigma X
		/// </summary>
		public FloatAttribute SigmaX
		{
			get => this.sigmaX;
			set => this.sigmaX = value;
		}

		/// <summary>
		/// Sigma Y
		/// </summary>
		public FloatAttribute SigmaY
		{
			get => this.sigmaY;
			set => this.sigmaY = value;
		}

		/// <summary>
		/// Tile Mode
		/// </summary>
		public EnumAttribute<SKShaderTileMode> TileMode
		{
			get => this.tileMode;
			set => this.tileMode = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.sigmaX = new FloatAttribute(Input, "sigmaX");
			this.sigmaY = new FloatAttribute(Input, "sigmaY");
			this.tileMode = new EnumAttribute<SKShaderTileMode>(Input, "tileMode");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.sigmaX.Export(Output);
			this.sigmaY.Export(Output);
			this.tileMode.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Blur(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Blur Dest)
			{
				Dest.sigmaX = this.sigmaX.CopyIfNotPreset();
				Dest.sigmaY = this.sigmaY.CopyIfNotPreset();
				Dest.tileMode = this.tileMode.CopyIfNotPreset();
			}
		}
	}
}
