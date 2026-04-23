using SkiaSharp;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of italic text in flowing text.
	/// </summary>
	public class Italic : EmbeddedText
	{
		/// <summary>
		/// Represents a segment of italic text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Italic(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Italic";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Italic(Document, Parent);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			SKFont FontBak = State.Font;

			State.Font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = FontBak.Size,
				Typeface = SKTypeface.FromFamilyName(FontBak.Typeface.FamilyName,
					FontBak.Typeface.FontWeight, FontBak.Typeface.FontWidth, 
					SKFontStyleSlant.Italic) ?? FontBak.Typeface
			};

			await base.MeasureSegments(Segments, State);

			State.Font = FontBak;
		}
	}
}
