using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class HSV : FunctionMultiVariate
	{
		public HSV(ScriptNode H, ScriptNode S, ScriptNode V, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { H, S, V }, FunctionMultiVariate.argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "H", "S", "V" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "HSV";
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double H = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double S = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double V = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

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

			return new ObjectValue(Graph.ToColorHSV(H, S, V));
		}
	}
}
