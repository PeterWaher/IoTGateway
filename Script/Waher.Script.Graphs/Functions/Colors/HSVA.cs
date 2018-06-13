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
	/// Returns a color value using HSV coordinates, and an alpha component.
	/// </summary>
	public class HSVA : FunctionMultiVariate
	{
		/// <summary>
		/// Returns a color value using HSV coordinates, and an alpha component.
		/// </summary>
		/// <param name="H">Hue</param>
		/// <param name="S">Saturation</param>
		/// <param name="V">Value</param>
		/// <param name="A">Alpha</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HSVA(ScriptNode H, ScriptNode S, ScriptNode V, ScriptNode A, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { H, S, V, A }, FunctionMultiVariate.argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "H", "S", "V", "A" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "HSVA";
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
			double V = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			int A = (int)(Expression.ToDouble(Arguments[3].AssociatedObjectValue) + 0.5);

			H = Math.IEEERemainder(H, 360);
			if (H < 0)
				H += 360;

			if (S < 0)
				S = 0;
			else if (S > 1)
				S = 1;

			if (V < 0)
				V = 0;
			else if (V > 1)
				V = 1;

			if (A < 0)
				A = 0;
			else if (A > 255)
				A = 255;

			return new ObjectValue(Graph.ToColorHSV(H, S, V, (byte)A));
		}
	}
}
