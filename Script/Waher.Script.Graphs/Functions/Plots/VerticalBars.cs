using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional vertical-bar chart.
	/// </summary>
	/// <example>
	/// x:=0..20;y:=sin(x);y2:=2*sin(x);VerticalBars("x"+x,y,rgba(255,0,0,128))+VerticalBars("x"+x,y2,rgba(0,0,255,128));
	/// </example>
	public class VerticalBars : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector };

		/// <summary>
		/// Plots a two-dimensional vertical-bar chart.
		/// </summary>
		/// <param name="Labels">Labels.</param>
		/// <param name="Values">Values.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VerticalBars(ScriptNode Labels, ScriptNode Values, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Labels, Values }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional vertical-bar chart.
		/// </summary>
		/// <param name="Labels">Labels.</param>
		/// <param name="Values">Values.</param>
		/// <param name="Color">Color</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VerticalBars(ScriptNode Labels, ScriptNode Values, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Labels, Values, Color }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "verticalbars"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "labels", "values", "color" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IVector Labels = Arguments[0] as IVector;
			if (Labels == null)
				throw new ScriptRuntimeException("Expected vector for Labels argument.", this);

			IVector Values = Arguments[1] as IVector;
			if (Values == null)
				throw new ScriptRuntimeException("Expected vector for Values argument.", this);

			int Dimension = Labels.Dimension;
			if (Values.Dimension != Dimension)
				throw new ScriptRuntimeException("Vector size mismatch.", this);

			IElement Color = Arguments.Length <= 2 ? null : Arguments[2];

			return new Graph2D(Labels, Values, this.DrawGraph, false, true, this,
				Color == null ? SKColors.Red : Color.AssociatedObjectValue);
		}

		private void DrawGraph(SKCanvas Canvas, SKPoint[] Points, object[] Parameters, SKPoint[] PrevPoints, object[] PrevParameters,
			DrawingArea DrawingArea)
		{
			SKPaint Brush = null;
			SKPath Path = null;

			try
			{
				Brush = new SKPaint()
				{
					Color = Graph.ToColor(Parameters[0]),
					Style = SKPaintStyle.Fill
				};
				Path = new SKPath();

				float HalfBarWidth = (DrawingArea.Width - Points.Length) * 0.45f / Points.Length;
				float x0, y0, x1, y1;
				int i, c;

				if (PrevPoints != null)
				{
					if ((c = PrevPoints.Length) != Points.Length)
						PrevPoints = null;
					else
					{
						for (i = 0; i < c; i++)
						{
							if (PrevPoints[i].X != Points[i].X)
							{
								PrevPoints = null;
								break;
							}
						}
					}
				}

				i = 0;
				foreach (SKPoint Point in Points)
				{
					x0 = Point.X - HalfBarWidth + 1;
					y0 = Point.Y;
					x1 = Point.X + HalfBarWidth - 1;
					y1 = PrevPoints != null ? PrevPoints[i++].Y : DrawingArea.OrigoY;

					Canvas.DrawRect(new SKRect(x0, y0, x1, y1), Brush);
				}

				Canvas.DrawPath(Path, Brush);
			}
			finally
			{
				if (Brush != null)
					Brush.Dispose();

				if (Path != null)
					Path.Dispose();
			}
		}

	}
}
