using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional layered area chart, based on a spline instead of a poly-line.
	/// https://en.wikipedia.org/wiki/Area_chart
	/// 
	/// Syntax:
	/// Plot2DLayeredCurveArea(x,y[,AreaColor])
	/// </summary>
	/// <example>
	/// x:=-10..10;y:=sin(x);y2:=2*sin(x/2);plot2dlayeredcurvearea(x,y,rgba(255,0,0,64))+plot2dlayeredcurvearea(x,y2,rgba(0,0,255,64))+plot2dcurve(x,y)+plot2dcurve(x,y2,"Blue")+scatter2d(x,y,"Red",5)+scatter2d(x,y2,"Blue",5)
	/// </example>
	public class Plot2DLayeredCurveArea : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector };

		/// <summary>
		/// Plots a two-dimensional layered area chart, based on a spline instead of a poly-line.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DLayeredCurveArea(ScriptNode X, ScriptNode Y, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional layered area chart, based on a spline instead of a poly-line.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Color">Area Color</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DLayeredCurveArea(ScriptNode X, ScriptNode Y, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Color, }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "plot2dlayeredcurvearea"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get
			{
				return new string[] { "plot2dlayeredsplinearea" };
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "y", "color" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IVector X = Arguments[0] as IVector;
			if (X is null)
				throw new ScriptRuntimeException("Expected vector for X argument.", this);

			IVector Y = Arguments[1] as IVector;
			if (Y is null)
				throw new ScriptRuntimeException("Expected vector for Y argument.", this);

			int Dimension = X.Dimension;
			if (Y.Dimension != Dimension)
				throw new ScriptRuntimeException("Vector size mismatch.", this);

			IElement AreaColor = Arguments.Length <= 2 ? null : Arguments[2];

			return new Graph2D(X, Y, this.DrawGraph, false, true, this,
				AreaColor is null ? new SKColor(SKColors.Red.Red, SKColors.Red.Green, SKColors.Red.Blue, 192) : AreaColor.AssociatedObjectValue);
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
					Style = SKPaintStyle.Fill,
					Color = Graph.ToColor(Parameters[0])
				};
				Path = new SKPath();

				Path = Plot2DCurve.CreateSpline(Points);

				IElement Zero;
				ISet Set = DrawingArea.MinY.AssociatedSet;
				IGroup Group = Set as IGroup;

				if (Group is null)
					Zero = new DoubleNumber(0);
				else
					Zero = Group.AdditiveIdentity;

				IVector XAxis = VectorDefinition.Encapsulate(new IElement[] { DrawingArea.MinX, DrawingArea.MaxX }, false, this) as IVector;
				IVector YAxis = VectorDefinition.Encapsulate(new IElement[] { Zero, Zero }, false, this) as IVector;

				PrevPoints = DrawingArea.Scale(XAxis, YAxis);

				if (DrawingArea.MinX is StringValue && DrawingArea.MaxX is StringValue)
				{
					PrevPoints[0].X = Points[0].X;
					PrevPoints[1].X = Points[Points.Length - 1].X;
				}

				PrevPoints = (SKPoint[])PrevPoints.Clone();
				Array.Reverse(PrevPoints);
				Plot2DCurve.CreateSpline(Path, PrevPoints);

				Path.LineTo(Points[0]);

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
