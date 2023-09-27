using SkiaSharp;
using System;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional horizontal line graph.
	/// </summary>
	public class Plot2DHorizontalLinePainter : SingleColorGraphPainter, IPainter2D
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
			SKPoint Last = default;
			bool First = true;
			int Mode = Math.Sign(Expression.ToDouble(Parameters[0]));

			try
			{
				Pen = Graph.ToPen(Parameters[1], Parameters[2]);
				Path = new SKPath();

				foreach (SKPoint Point in Points)
				{
					if (First)
					{
						First = false;
						Path.MoveTo(Point);
					}
					else if (Point.Y == Last.Y || Point.X == Last.X)
						Path.LineTo(Point);
					else
					{
						switch (Mode)
						{
							case -1:
								Path.LineTo(Last.X, Point.Y);
								Path.LineTo(Point);
								break;

							case 0:
								float xm = (Last.X + Point.X) / 2;
								Path.LineTo(xm, Last.Y);
								Path.LineTo(xm, Point.Y);
								Path.LineTo(Point);
								break;

							case 1:
								Path.LineTo(Point.X, Last.Y);
								Path.LineTo(Point);
								break;
						}

						Path.LineTo(Point);
					}

					Last = Point;
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
