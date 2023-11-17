using SkiaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of underline text in flowing text.
	/// </summary>
	public class Underline : EmbeddedText
	{
		/// <summary>
		/// Represents a segment of underline text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Underline(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Underline";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Underline(Document, Parent);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(List<Segment> Segments, DrawingState State)
		{
			int i = Segments.Count;

			await base.MeasureSegments(Segments, State);

			int c = Segments.Count;

			for (; i < c; i++)
			{
				Segment Segment = Segments[i];
				Segment.LinePos = 2;
			}
		}
	}
}
