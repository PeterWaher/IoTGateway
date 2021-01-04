using System;
using SkiaSharp;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Interface for 2D graph drawing functions.
	/// </summary>
	public interface IPainter2D
	{
		/// <summary>
		/// Draws the graph on a canvas.
		/// </summary>
		/// <param name="Canvas">Canvas to draw on.</param>
		/// <param name="Points">Points to draw.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <param name="PrevPoints">Points of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevParameters">Parameters of previous graph of same type (if available), null (if not available).</param>
		/// <param name="DrawingArea">Current drawing area.</param>
		void DrawGraph(SKCanvas Canvas, SKPoint[] Points, object[] Parameters, SKPoint[] PrevPoints, object[] PrevParameters,
			DrawingArea DrawingArea);
	}
}
