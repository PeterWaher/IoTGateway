using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Interface for path segment nodes
	/// </summary>
	public interface ISegment : ILayoutElement
	{
		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		void Measure(DrawingState State, PathState PathState);
	}
}
