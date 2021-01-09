using System;
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
			if (!(Arguments[0] is IVector Labels))
				throw new ScriptRuntimeException("Expected vector for Labels argument.", this);

			if (!(Arguments[1] is IVector Values))
				throw new ScriptRuntimeException("Expected vector for Values argument.", this);

			int Dimension = Labels.Dimension;
			if (Values.Dimension != Dimension)
				throw new ScriptRuntimeException("Vector size mismatch.", this);

			IElement Color = Arguments.Length <= 2 ? null : Arguments[2];

			return new Graph2D(Labels, Values, new VerticalBarsPainter(), false, true, this,
				Color is null ? Graph.DefaultColor : Color.AssociatedObjectValue);
		}
	}
}
