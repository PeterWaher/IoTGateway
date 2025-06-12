using SkiaSharp;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of bold text in flowing text.
	/// </summary>
	public class Bold : EmbeddedText
	{
		/// <summary>
		/// Represents a segment of bold text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Bold(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Bold";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Bold(Document, Parent);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			SKFont Bak = State.Font;
			SKPaint Bak2 = State.Text;

			State.Font = new SKFont()
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
				Size = Bak.Size,
				Typeface = SKTypeface.FromFamilyName(Bak.Typeface.FamilyName,
					(int)SKFontStyleWeight.Bold, Bak.Typeface.FontWidth, Bak.Typeface.FontSlant)
			};

			State.Text = State.Text.Clone();
			State.Text.Typeface = State.Font.Typeface;

			await base.MeasureSegments(Segments, State);

			State.Font = Bak;
			State.Text = Bak2;
		}
	}
}
