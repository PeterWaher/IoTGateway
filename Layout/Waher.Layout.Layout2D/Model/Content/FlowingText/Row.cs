using SkiaSharp;
using System;
using System.Text;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Contains information about a row of text in a paragraph of flowing text.
	/// </summary>
	public class Row : IDisposable
	{
		/// <summary>
		/// Segments
		/// </summary>
		public Segment[] Segments { get; set; }

		/// <summary>
		/// Bounds of segment.
		/// </summary>
		public SKRect Bounds { get; set; }

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
		/// Position on base-line.
		/// </summary>
		public float? BaseLine { get; set; }

		/// <summary>
		/// <see cref="IDisposable"/>
		/// </summary>
		public void Dispose()
		{
			if (!(this.Segments is null))
			{
				foreach (Segment Segment in this.Segments)
					Segment.Dispose();

			}

			this.Segments = null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (!(this.Segments is null))
			{
				bool AddSpace = false;

				foreach (Segment Segment in this.Segments)
				{
					if (AddSpace)
						sb.Append(' ');

					sb.Append(Segment.Text);
					AddSpace = Segment.SpaceAfter;
				}
			}

			return sb.ToString();
		}
	}
}
