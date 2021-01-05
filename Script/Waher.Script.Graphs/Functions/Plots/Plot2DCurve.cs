using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional curve.
	/// </summary>
	/// <example>
	/// x:=-10..10;
	/// y:=sin(x);
	/// plot2dcurve(x,y);
	/// </example>
	public class Plot2DCurve : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes4Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector };

		/// <summary>
		/// Plots a two-dimensional curve.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DCurve(ScriptNode X, ScriptNode Y, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional curve.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Color">Color</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DCurve(ScriptNode X, ScriptNode Y, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Color }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional curve.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Color">Color</param>
		/// <param name="Size">Size</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DCurve(ScriptNode X, ScriptNode Y, ScriptNode Color, ScriptNode Size, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Color, Size }, argumentTypes4Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "plot2dcurve"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get
			{
				return new string[] { "plot2dspline" };
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "y", "color", "size" }; }
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

			IElement Color = Arguments.Length <= 2 ? null : Arguments[2];
			IElement Size = Arguments.Length <= 3 ? null : Arguments[3];

			return new Graph2D(X, Y, new Plot2DCurvePainter(), false, false, this,
				Color is null ? Graph.DefaultColor : Color.AssociatedObjectValue,
				Size is null ? 2.0 : Size.AssociatedObjectValue);
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		[Obsolete("Method moved to the Plot2DCurvePainter class.")]
		public static SKPath CreateSpline(params SKPoint[] Points)
		{
			return Plot2DCurvePainter.CreateSpline(Points);
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		[Obsolete("Method moved to the Plot2DCurvePainter class.")]
		public static SKPath CreateSpline(SKPath AppendTo, params SKPoint[] Points)
		{
			return Plot2DCurvePainter.CreateSpline(AppendTo, Points);
		}

		/// <summary>
		/// Gets a set of coefficients for cubic Bezier curves, forming a spline, one coordinate at a time.
		/// </summary>
		/// <param name="V">One set of coordinates.</param>
		/// <param name="A">Corresponding coefficients for first control points.</param>
		/// <param name="B">Corresponding coefficients for second control points.</param>
		[Obsolete("Method moved to the Plot2DCurvePainter class.")]
		public static void GetCubicBezierCoefficients(double[] V, out double[] A, out double[] B)
		{
			Plot2DCurve.GetCubicBezierCoefficients(V, out A, out B);
		}

	}
}
