using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional polygon.
	/// </summary>
	public class Polygon2DPainter : IPainter2D
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
			SKColor Color = Graph.ToColor(Parameters[0]);
			SKPaint Brush = new SKPaint
			{
				FilterQuality = SKFilterQuality.High,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Color
			};

			SKPath Path = new SKPath();
			bool First = true;

			foreach (SKPoint P in Points)
			{
				if (First)
				{
					First = false;
					Path.MoveTo(P);
				}
				else
					Path.LineTo(P);
			}

			Path.Close();

			Canvas.DrawPath(Path, Brush);

			Brush.Dispose();
			Path.Dispose();
		}
	}
}
