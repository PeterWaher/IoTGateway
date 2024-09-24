using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional horizontal line.
	/// </summary>
	/// <example>
	/// x:=-10..10;
	/// y:=sin(x);
	/// plot2dhline(x,y,0,"Red",5)+scatter2d(x,y,"Blue",5)+plot2dline(x,y,"Blue",1);
	/// </example>
	public class Plot2DHorizontalLine : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes5Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes4Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar };

		/// <summary>
		/// Plots a two-dimensional horizontal line.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Mode">Mode: Negative: Value begins at last point, zero: change occurs between points, Positive: Value begins at new point.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DHorizontalLine(ScriptNode X, ScriptNode Y, ScriptNode Mode, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Mode }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional horizontal line.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Mode">Mode: Negative: Value begins at last point, zero: change occurs between points, Positive: Value begins at new point.</param>
		/// <param name="Color">Color</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DHorizontalLine(ScriptNode X, ScriptNode Y, ScriptNode Mode, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Mode, Color }, argumentTypes4Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a two-dimensional horizontal line.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Mode">Mode: Negative: Value begins at last point, zero: change occurs between points, Positive: Value begins at new point.</param>
		/// <param name="Color">Color</param>
		/// <param name="Size">Size</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Plot2DHorizontalLine(ScriptNode X, ScriptNode Y, ScriptNode Mode, ScriptNode Color, ScriptNode Size, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Mode, Color, Size }, argumentTypes5Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Plot2DHorizontalLine);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "plot2dhline" };

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

			IElement Mode = Arguments[2];
			IElement Color = Arguments.Length <= 3 ? null : Arguments[3];
			IElement Size = Arguments.Length <= 4 ? null : Arguments[4];

			return new Graph2D(Variables, X, Y, new Plot2DHorizontalLinePainter(), false, false, this, Mode.AssociatedObjectValue,
				Color?.AssociatedObjectValue ?? Graph.DefaultColor, Size?.AssociatedObjectValue ?? 2.0);
		}
	}
}
