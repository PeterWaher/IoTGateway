using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Script;
using Waher.Script.Graphs;

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
		public override Task FromXml(XmlElement Input)
		{
			this.cid = new StringAttribute(Input, "cid", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.cid?.Export(Output);
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
				Dest.cid = this.cid?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override async Task<SKImage> LoadImage(DrawingState State)
		{
			string ContentId = await this.cid.Evaluate(State.Session, string.Empty);
			if (string.IsNullOrEmpty(ContentId))
				return null;

			if (this.Document.TryGetContent(ContentId, out object Content))
				return Content as SKImage;

			if (State.Session.TryGetVariable(ContentId, out Variable v))
			{
				object Obj = v.ValueObject;

				if (Obj is SKImage Image)
					return Image;

				if (Obj is PixelInformation Pixels)
					return Pixels.CreateBitmap();

				if (Obj is Graph Graph)
				{
					Pixels = Graph.CreatePixels(State.Session);
					return Pixels.CreateBitmap();
				}
			}

			return null;
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.cid?.ExportState(Output);
		}
	}
}
