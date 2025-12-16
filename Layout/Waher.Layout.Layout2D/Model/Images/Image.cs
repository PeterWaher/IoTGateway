using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Figures;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// Abstract base class for images.
	/// </summary>
	public abstract class Image : FigurePoint2
	{
		private BooleanAttribute keepAspectRatio;

		/// <summary>
		/// Abstract base class for images.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Image(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
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
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.image?.Dispose();
			this.image = null;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.keepAspectRatio = new BooleanAttribute(Input, "keepAspectRatio", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.keepAspectRatio?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Image Dest)
				Dest.keepAspectRatio = this.keepAspectRatio?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.keepAspectRatio?.ExportState(Output);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			if (this.image is null && !this.loadStarted)
			{
				this.loadStarted = true;
				this.image = await this.LoadImage(State);
			}

			if (!(this.image is null))
			{
				this.Width = this.ExplicitWidth = this.image.Width;
				this.Height = this.ExplicitHeight = this.image.Height;
			}
			else
			{
				ILayoutElement Alternative = this.GetAlternative(State);

				if (!(Alternative is null))
				{
					this.Width = Alternative.Width;
					this.ExplicitWidth = Alternative.ExplicitWidth;
					this.Height = Alternative.Height;
					this.ExplicitHeight = Alternative.ExplicitHeight;
				}
			}
		
			await base.DoMeasureDimensions(State);
		}

		/// <summary>
		/// Loaded image
		/// </summary>
		protected SKImage image = null;

		private bool loadStarted = false;

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected abstract Task<SKImage> LoadImage(DrawingState State);

		/// <summary>
		/// Gets an alternative representation, in case the image is not available.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Alternative representation, if available, null otherwise.</returns>
		protected virtual ILayoutElement GetAlternative(DrawingState State)
		{
			return null;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (!(this.image is null))
			{
				State.Canvas.DrawImage(this.image, new SKRect(
					this.xCoordinate, this.yCoordinate,
					this.xCoordinate2, this.yCoordinate2));
			}

			await base.Draw(State);
		}
	}
}
