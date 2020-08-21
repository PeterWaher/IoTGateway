using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// An image provided by the caller, identified by a content id.
	/// </summary>
	public class ImageInternal : Image
	{
		private StringAttribute cid;

		/// <summary>
		/// An image provided by the caller, identified by a content id.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ImageInternal(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ImageInternal";

		/// <summary>
		/// Content ID
		/// </summary>
		public StringAttribute CidAttribute
		{
			get => this.cid;
			set => this.cid = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.cid = new StringAttribute(Input, "cid");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.cid.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ImageInternal(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is ImageInternal Dest)
				Dest.cid = this.cid.CopyIfNotPreset();
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override SKImage LoadImage(DrawingState State)
		{
			if (this.cid.TryEvaluate(State.Session, out string ContentId) &&
				this.Document.TryGetContent(ContentId, out object Content) &&
				Content is SKImage Image)
			{
				return Image;
			}
			else
				return null;
		}
	}
}
