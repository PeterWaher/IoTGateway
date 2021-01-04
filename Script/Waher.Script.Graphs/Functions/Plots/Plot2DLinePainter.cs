using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional line.
	/// </summary>
	public class Plot2DLinePainter : IPainter2D
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
		public void DrawGraph(SKCanvas Canvas, SKPoint[] Points, object[] Parameters, SKPoint[] PrevPoints, object[] PrevParameters,
			DrawingArea DrawingArea)
		{
			SKPaint Pen = null;
			SKPath Path = null;
			bool First = true;

			try
			{
				Pen = Graph.ToPen(Parameters[0], Parameters[1]);
				Path = new SKPath();

				foreach (SKPoint Point in Points)
				{
					if (First)
					{
						First = false;
						Path.MoveTo(Point);
					}
					else
						Path.LineTo(Point);
				}

				Canvas.DrawPath(Path, Pen);
			}
			finally
			{
				Pen?.Dispose();
				Path?.Dispose();
			}
		}

	}
}
