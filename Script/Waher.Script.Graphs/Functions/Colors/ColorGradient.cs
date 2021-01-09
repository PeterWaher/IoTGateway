using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	/// <summary>
	/// Returns a color from a color gradient defined by a vector of colors and an interpolation constant 0&lt;=`p`&lt;=1.
	/// </summary>
	public class ColorGradient : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar };

		/// <summary>
		/// Returns a color from a color gradient defined by a vector of colors and an interpolation constant 0&lt;=`p`&lt;=1.
		/// </summary>
		/// <param name="Colors">Second color, or image.</param>
		/// <param name="p">Blending factor in [0,1].</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ColorGradient(ScriptNode Colors, ScriptNode p, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Colors, p }, argumentTypes, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Colors", "p" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "ColorGradient";
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IVector Colors = Arguments[0] as IVector;
			int c = Colors.Dimension;
			if (c == 0)
				throw new ScriptRuntimeException("No colors defined.", this);

			if (c == 1)
				return Colors.GetElement(0);

			double p = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			p *= c - 1;

			int Offset = (int)p;

			if (Offset < 0)
				Offset = 0;
			else if (Offset > c - 2)
				Offset = c - 2;

			p -= Offset;

			SKColor c1 = Graph.ToColor(Colors.GetElement(Offset).AssociatedObjectValue);
			SKColor c2 = Graph.ToColor(Colors.GetElement(Offset + 1).AssociatedObjectValue);

			return new ObjectValue(Blend.BlendColors(c1, c2, p));
		}
	}
}
