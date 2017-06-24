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
	/// Returns a color value using HSL coordinates, and an alpha component.
	/// </summary>
	public class HSLA : FunctionMultiVariate
	{
		/// <summary>
		/// Returns a color value using HSL coordinates, and an alpha component.
		/// </summary>
		/// <param name="H">Hue</param>
		/// <param name="S">Saturation</param>
		/// <param name="L">Light</param>
		/// <param name="A">Alpha</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HSLA(ScriptNode H, ScriptNode S, ScriptNode L, ScriptNode A, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { H, S, L, A }, FunctionMultiVariate.argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "H", "S", "L", "A" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "HSLA";
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
			int A = (int)(Expression.ToDouble(Arguments[3].AssociatedObjectValue) + 0.5);

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

			if (A < 0)
				A = 0;
			else if (A > 255)
				A = 1;

			return new ObjectValue(Graph.ToColorHSL((byte)H, (byte)S, (byte)L, (byte)A));
		}
	}
}
