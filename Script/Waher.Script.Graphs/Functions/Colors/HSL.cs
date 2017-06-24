using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	/// <summary>
	/// Returns a color value using HSL coordinates.
	/// </summary>
	public class HSL : FunctionMultiVariate
	{
		/// <summary>
		/// Returns a color value using HSL coordinates.
		/// </summary>
		/// <param name="H">Hue</param>
		/// <param name="S">Saturation</param>
		/// <param name="L">Light</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HSL(ScriptNode H, ScriptNode S, ScriptNode L, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { H, S, L }, FunctionMultiVariate.argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "H", "S", "L" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "HSL";
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
			double H = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double S = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double L = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

			H = Math.IEEERemainder(H, 360);
			if (H < 0)
				H += 360;

			if (S < 0)
				S = 0;
			else if (S > 1)
				S = 1;

			if (L < 0)
				L = 0;
			else if (L > 1)
				L = 1;

			return new ObjectValue(Graph.ToColorHSL(H, S, L));
		}
	}
}
