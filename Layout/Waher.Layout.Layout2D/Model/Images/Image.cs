using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Backgrounds;
using Waher.Layout.Layout2D.Model.Figures;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// Abstract base class for images.
	/// </summary>
	public abstract class Image : FigurePoint2
	{
		private BooleanAttribute keepAspectRatio;
		private BooleanAttribute clip;

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
		/// If image should be clipped to the defined area.
		/// </summary>
		public BooleanAttribute Clip
		{
			get => this.clip;
			set => this.clip = value;
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
			this.clip = new BooleanAttribute(Input, "clip", this.Document);

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
			this.clip?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Image Dest)
			{
				Dest.keepAspectRatio = this.keepAspectRatio?.CopyIfNotPreset(Destination.Document);
				Dest.clip = this.clip?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.keepAspectRatio?.ExportState(Output);
			this.clip?.ExportState(Output);
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
				if (this.XAttribute.Undefined || this.X2Attribute.Undefined)
					this.Width = this.ExplicitWidth = this.image.Width;

				if (this.YAttribute.Undefined || this.Y2Attribute.Undefined)
					this.Height = this.ExplicitHeight = this.image.Height;

				this.keepAspectRatioValue = await this.keepAspectRatio.Evaluate(State.Session, false);
				this.clipValue = await this.clip.Evaluate(State.Session, false);
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

		private bool keepAspectRatioValue = false;
		private bool clipValue = false;
		private bool flipX = false;
		private bool flipY = false;
		private SKRect imageSourcePosition;
		private SKRect imageDestinationPosition;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			float ImageWidth = this.image.Width;
			float ImageHeight = this.image.Height;
			float Temp;

			if (this.flipX = this.xCoordinate > this.xCoordinate2)
			{
				Temp = this.xCoordinate;
				this.xCoordinate = this.xCoordinate2;
				this.xCoordinate2 = Temp;
			}

			if (this.flipY = this.yCoordinate > this.yCoordinate2)
			{
				Temp = this.yCoordinate;
				this.yCoordinate = this.yCoordinate2;
				this.yCoordinate2 = Temp;
			}

			if (this.keepAspectRatioValue &&
				!(this.image is null) &&
				ImageWidth > 0 &&
				ImageHeight > 0 &&
				this.xCoordinate != this.xCoordinate2 &&
				this.yCoordinate != this.yCoordinate2)
			{
				float AreaWidth = this.xCoordinate2 - this.xCoordinate;
				float AreaHeight = this.yCoordinate2 - this.yCoordinate;
				float ScaleX = AreaWidth / ImageWidth;
				float ScaleY = AreaHeight / ImageHeight;
				float Scale;
				float XOffset;
				float YOffset;

				if (this.clipValue)
				{
					this.imageDestinationPosition = new SKRect(
						this.xCoordinate, this.yCoordinate,
						this.xCoordinate2 - this.xCoordinate, this.yCoordinate2 - this.yCoordinate);

					Scale = Math.Max(ScaleX, ScaleY);
					AreaWidth /= Scale;
					AreaHeight /= Scale;
					XOffset = (ImageWidth - AreaWidth) / 2;
					YOffset = (ImageHeight - AreaHeight) / 2;

					this.imageSourcePosition = new SKRect(
						XOffset,
						YOffset,
						ImageWidth - XOffset,
						ImageHeight - YOffset);
				}
				else
				{
					this.imageSourcePosition = new SKRect(0, 0, ImageWidth, ImageHeight);

					Scale = Math.Min(ScaleX, ScaleY);
					ImageWidth *= Scale;
					ImageHeight *= Scale;
					XOffset = (AreaWidth - ImageWidth) / 2;
					YOffset = (AreaHeight - ImageHeight) / 2;

					this.imageDestinationPosition = new SKRect(
						this.xCoordinate + XOffset,
						this.yCoordinate + YOffset,
						this.xCoordinate2 - XOffset,
						this.yCoordinate2 - YOffset);
				}
			}
			else
			{
				this.imageSourcePosition = new SKRect(0, 0, ImageWidth, ImageHeight);

				this.imageDestinationPosition = new SKRect(
					this.xCoordinate, this.yCoordinate,
					this.xCoordinate2, this.yCoordinate2);
			}

			base.MeasurePositions(State);
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
			State.Canvas.Save();

			if (this.flipX)
			{
				State.Canvas.Scale(-1, 1);
				State.Canvas.Translate(-this.imageDestinationPosition.Left - this.imageDestinationPosition.Right, 0);
			}

			if (this.flipY)
			{
				State.Canvas.Scale(1, -1);
				State.Canvas.Translate(0, -this.imageDestinationPosition.Top - this.imageDestinationPosition.Bottom);
			}

			if (!(this.image is null))
			{
				State.Canvas.DrawImage(this.image, this.imageSourcePosition,
					this.imageDestinationPosition);
			}

			State.Canvas.Restore();

			await base.Draw(State);
		}
	}
}
