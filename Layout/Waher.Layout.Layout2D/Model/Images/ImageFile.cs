using System.IO;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Runtime.IO;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// An image defined in a file.
	/// </summary>
	public class ImageFile : Image
	{
		private StringAttribute fileName;

		/// <summary>
		/// An image defined in a file.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ImageFile(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ImageFile";

		/// <summary>
		/// Filename
		/// </summary>
		public StringAttribute FileNameAttribute
		{
			get => this.fileName;
			set => this.fileName = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.fileName = new StringAttribute(Input, "fileName", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.fileName?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ImageFile(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is ImageFile Dest)
				Dest.fileName = this.fileName?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override async Task<SKImage> LoadImage(DrawingState State)
		{
			string FileName = await this.fileName.Evaluate(State.Session, string.Empty);
			if (!string.IsNullOrEmpty(FileName) && File.Exists(FileName))
			{
				byte[] Bin = await Files.ReadAllBytesAsync(FileName);
				SKBitmap Bitmap = SKBitmap.Decode(Bin);
				return SKImage.FromBitmap(Bitmap);
			}
			else
				return null;
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.fileName?.ExportState(Output);
		}
	}
}
