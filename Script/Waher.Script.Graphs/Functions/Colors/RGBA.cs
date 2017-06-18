using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class RGBA : FunctionMultiVariate
	{
		public RGBA(ScriptNode R, ScriptNode G, ScriptNode B, ScriptNode A, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { R, G, B, A }, FunctionMultiVariate.argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "R", "G", "B", "A" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "RGBA";
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int R = (int)(Expression.ToDouble(Arguments[0].AssociatedObjectValue) + 0.5);
			int G = (int)(Expression.ToDouble(Arguments[1].AssociatedObjectValue) + 0.5);
			int B = (int)(Expression.ToDouble(Arguments[2].AssociatedObjectValue) + 0.5);
			int A = (int)(Expression.ToDouble(Arguments[3].AssociatedObjectValue) + 0.5);

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

			if (A < 0)
				A = 0;
			else if (A > 255)
				A = 255;

			return new ObjectValue(new SKColor((byte)R, (byte)G, (byte)B, (byte)A));
		}
	}
}
