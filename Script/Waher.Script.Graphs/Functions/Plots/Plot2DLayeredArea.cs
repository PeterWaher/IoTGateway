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
	/// Plots a two-dimensional layered area chart.
	/// https://en.wikipedia.org/wiki/Area_chart
	/// 
	/// Syntax:
	/// Plot2DLayeredArea(x,y[,AreaColor])
	/// </summary>
	/// <example>
	/// x:=-10..10;y:=sin(x);y2:=2*sin(x/2);plot2dlayeredarea(x,y,rgba(255,0,0,64))+plot2dlayeredarea(x,y2,rgba(0,0,255,64))+plot2dline(x,y)+plot2dline(x,y2,"Blue")
	/// </example>
	public class Plot2DLayeredArea : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector };

		/// <summary>
		/// Plots a two-dimensional layered area chart.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DLayeredArea(ScriptNode X, ScriptNode Y, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional layered area chart.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Color">Area Color</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DLayeredArea(ScriptNode X, ScriptNode Y, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Color, }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "plot2dlayeredarea"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get
			{
				return new string[] { "plot2dlayeredlinearea" };
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
			if (!(Arguments[0] is IVector X))
				throw new ScriptRuntimeException("Expected vector for X argument.", this);

			if (!(Arguments[1] is IVector Y))
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
			bool First = true;

			try
			{
				Brush = new SKPaint()
				{
					Style = SKPaintStyle.Fill,
					Color = Graph.ToColor(Parameters[0])
				};
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

				IElement Zero;
				ISet Set = DrawingArea.MinY.AssociatedSet;

				if (Set is IGroup Group)
					Zero = Group.AdditiveIdentity;
				else
					Zero = new DoubleNumber(0);

				IVector XAxis = VectorDefinition.Encapsulate(new IElement[] { DrawingArea.MinX, DrawingArea.MaxX }, false, this) as IVector;
				IVector YAxis = VectorDefinition.Encapsulate(new IElement[] { Zero, Zero }, false, this) as IVector;

				PrevPoints = DrawingArea.Scale(XAxis, YAxis);

				if (DrawingArea.MinX is StringValue && DrawingArea.MaxX is StringValue)
				{
					PrevPoints[0].X = Points[0].X;
					PrevPoints[1].X = Points[Points.Length - 1].X;
				}

				int i = PrevPoints.Length;

				while (--i >= 0)
					Path.LineTo(PrevPoints[i]);

				Path.LineTo(Points[0]);

				Canvas.DrawPath(Path, Brush);
			}
			finally
			{
				if (!(Brush is null))
					Brush.Dispose();

				if (!(Path is null))
					Path.Dispose();
			}
		}

	}
}
