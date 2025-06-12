using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of strikeThrough text in flowing text.
	/// </summary>
	public class StrikeThrough : EmbeddedText
	{
		/// <summary>
		/// Represents a segment of strikeThrough text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public StrikeThrough(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "StrikeThrough";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new StrikeThrough(Document, Parent);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			int i = Segments.Count;

			await base.MeasureSegments(Segments, State);

			int c = Segments.Count;

			for (; i < c; i++)
			{
				Segment Segment = Segments[i];
				Segment.LinePos = -State.Font.Size / 3;
			}
		}
	}
}
