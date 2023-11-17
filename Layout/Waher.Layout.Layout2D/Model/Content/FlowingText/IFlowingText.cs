using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Base interface for flowing text
	/// </summary>
	public interface IFlowingText : ILayoutElement
	{
		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		Task MeasureSegments(List<Segment> Segments, DrawingState State);
	}
}
