using System;
using System.Collections.Generic;
using System.Drawing;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class RGB : FunctionMultiVariate
	{
		public RGB(ScriptNode R, ScriptNode G, ScriptNode B, int Start, int Length)
			: base(new ScriptNode[] { R, G, B }, FunctionMultiVariate.argumentTypes3Scalar, Start, Length)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "R", "G", "B" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "RGB";
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int R = (int)(Expression.ToDouble(Arguments[0].AssociatedObjectValue) + 0.5);
			int G = (int)(Expression.ToDouble(Arguments[1].AssociatedObjectValue) + 0.5);
			int B = (int)(Expression.ToDouble(Arguments[2].AssociatedObjectValue) + 0.5);

			if (R < 0)
				R = 0;
			else if (R > 255)
				R = 255;

			if (G < 0)
				G = 0;
			else if (G > 255)
				G = 255;

			if (B < 0)
				B = 0;
			else if (B > 255)
				B = 255;

			return new ObjectValue(Color.FromArgb(R, G, B));
		}
	}
}
