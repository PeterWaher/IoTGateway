﻿using SkiaSharp;
using System;
using System.Threading.Tasks;
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
