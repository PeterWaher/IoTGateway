using SkiaSharp;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Interface for path segment nodes
	/// </summary>
	public interface ISegment : ILayoutElement
	{
		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		Task Measure(DrawingState State, PathState PathState);

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		Task Draw(DrawingState State, PathState PathState, SKPath Path);
	}
}
