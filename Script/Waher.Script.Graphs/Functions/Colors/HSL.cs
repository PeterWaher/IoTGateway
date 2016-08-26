using System;
using System.Collections.Generic;
using System.Drawing;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class HSL : FunctionMultiVariate
	{
		public HSL(ScriptNode H, ScriptNode S, ScriptNode L, int Start, int Length)
			: base(new ScriptNode[] { H, S, L }, FunctionMultiVariate.argumentTypes3Scalar, Start, Length)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "H", "S", "L" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "HSL";
			}
		}

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
