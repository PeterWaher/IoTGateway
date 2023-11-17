using SkiaSharp;
using System;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Contains information about a segment of flowing text.
	/// </summary>
	public class Segment : IDisposable
	{
		/// <summary>
		/// Text segment.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Font state.
		/// </summary>
		public SKFont Font { get; set; }

		/// <summary>
		/// Paint state.
		/// </summary>
		public SKPaint Paint { get; set; }

		/// <summary>
		/// Bounds of segment.
		/// </summary>
		public SKRect Bounds { get; set; }

		/// <summary>
		/// Bounds of a space character.
		/// </summary>
		public SKRect SpaceBounds { get; set; }

		/// <summary>
		/// Width of segment.
		/// </summary>
		public float Width => this.Bounds.Width;

		/// <summary>
		/// Height of segment.
		/// </summary>
		public float Height => this.Bounds.Height;

		/// <summary>
		/// Top of segment.
		/// </summary>
		public float Top => this.Bounds.Top;

		/// <summary>
		/// Bottom of segment.
		/// </summary>
		public float Bottom => this.Bounds.Bottom;

		/// <summary>
		/// Width of a space character.
		/// </summary>
		public float SpaceWidth => this.SpaceBounds.Width;

		/// <summary>
		/// Height of a space character.
		/// </summary>
		public float SpaceHeight => this.SpaceBounds.Height;

		/// <summary>
		/// Position on optional horizontal line.
		/// </summary>
		public float? LinePos { get; set; }

		/// <summary>
		/// Y-offset of segment.
		/// </summary>
		public float DeltaY { get; set; }

		/// <summary>
		/// If there's white-space after the segment
		/// </summary>
		public bool SpaceAfter { get; set; }

		/// <summary>
		/// <see cref="IDisposable"/>
		/// </summary>
		public void Dispose()
		{
			this.Paint?.Dispose();
			this.Paint = null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Text;
		}

		/// <summary>
		/// Moves the segment along the Y-axis.
		/// </summary>
		/// <param name="DeltaY">Delta-Y</param>
		public void TranslateY(float DeltaY)
		{
			if (DeltaY != 0)
			{
				this.Bounds = new SKRect(this.Bounds.Left, this.Bounds.Top + DeltaY,
					this.Bounds.Right, this.Bounds.Bottom + DeltaY);
			}
		}
	}
}
