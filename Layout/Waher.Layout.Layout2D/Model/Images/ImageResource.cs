using System;
using System.Reflection;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// An image defined in an embedded resource.
	/// </summary>
	public class ImageResource : Image
	{
		private StringAttribute resource;

		/// <summary>
		/// An image defined in an embedded resource.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ImageResource(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ImageResource";

		/// <summary>
		/// Resource
		/// </summary>
		public StringAttribute Resource
		{
			get => this.resource;
			set => this.resource = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.resource = new StringAttribute(Input, "resource");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.resource.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ImageResource(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is ImageResource Dest)
				Dest.resource = this.resource.CopyIfNotPreset();
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override SKImage LoadImage(DrawingState State)
		{
			if (this.resource.TryEvaluate(State.Session, out string ResourceName))
			{
				byte[] Bin = Resources.LoadResource(ResourceName);
				SKBitmap Bitmap = SKBitmap.Decode(Bin);
				return SKImage.FromBitmap(Bitmap);
			}
			else
				return null;
		}

	}
}
