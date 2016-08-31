using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Functions.Transforms
{
	public class Rotate2D : FunctionOneScalarVariable
	{
		public Rotate2D(ScriptNode Angle, int Start, int Length, Expression Expression)
			: base(Angle, Start, Length, Expression)
		{
		}

		public override string FunctionName
		{
			get
			{
				return "Rotate2D";
			}
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "angle" };
			}
		}

		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			double S = Math.Sin(Argument);
			double C = Math.Cos(Argument);
			double[,] E = new double[,] { { C, S }, { -S, C } };

			return new DoubleMatrix(E);
		}
	}
}
