using System;
using SkiaSharp;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional scatter graph.
	/// </summary>
	public class Scatter2DPainter : SingleColorGraphPainter, IPainter2D
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
			float Size = (float)Expression.ToDouble(Parameters[1]);
			SKPaint Brush = new SKPaint
			{
				FilterQuality = SKFilterQuality.High,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = Color
			};

			foreach (SKPoint P in Points)
				Canvas.DrawCircle(P.X, P.Y, Size, Brush);

			Brush.Dispose();
		}
	}
}
