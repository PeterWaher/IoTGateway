using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Functions.Vectors;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of subscript text in flowing text.
	/// </summary>
	public class Subscript : EmbeddedText
	{
		/// <summary>
		/// Represents a segment of subscript text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Subscript(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Subscript";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Subscript(Document, Parent);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(List<Segment> Segments, DrawingState State)
		{
			int i = Segments.Count;

			SKFont Bak = State.Font;
			SKPaint Bak2 = State.Text;

			State.Font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = Bak.Size * 0.6f,
				Typeface = Bak.Typeface
			};

			State.Text = State.Text.Clone();
			State.Text.TextSize = Bak.Size * 0.6f;

			await base.MeasureSegments(Segments, State);

			State.Font = Bak;
			State.Text = Bak2;

			int c = Segments.Count;

			for (; i < c; i++)
			{
				Segment Segment = Segments[i];
				Segment.DeltaY += 0.25f * Bak.Size;
			}
		}
	}
}
