using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Figures;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// Abstract base class for images.
	/// </summary>
	public abstract class Image : FigurePoint2
	{
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
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.image?.Dispose();
			this.image = null;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			if (this.image is null && !this.loadStarted)
			{
				this.loadStarted = true;
				this.image = this.LoadImage(State);
			}

			base.MeasureDimensions(State);
		}

		/// <summary>
		/// Loaded image
		/// </summary>
		protected SKImage image = null;

		private bool loadStarted = false;

		/// <summary>
		/// Default width of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Width</returns>
		public override float GetDefaultWidth(DrawingState State)
		{
			return this.image?.Width ?? base.GetDefaultWidth(State);
		}

		/// <summary>
		/// Default height of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Height</returns>
		public override float GetDefaultHeight(DrawingState State)
		{
			return this.image?.Height ?? base.GetDefaultHeight(State);
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected abstract SKImage LoadImage(DrawingState State);

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (!(this.image is null))
			{
				State.Canvas.DrawImage(this.image, new SKRect(
					this.xCoordinate, this.yCoordinate,
					this.xCoordinate2, this.yCoordinate2));
			}
		
			base.Draw(State);
		}
	}
}
