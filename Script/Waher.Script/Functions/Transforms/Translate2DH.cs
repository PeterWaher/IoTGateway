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
	public class Translate2DH : FunctionTwoScalarVariables
	{
		public Translate2DH(ScriptNode Dx, ScriptNode Dy, int Start, int Length, Expression Expression)
			: base(Dx, Dy, Start, Length, Expression)
		{
		}

		public override string FunctionName
		{
			get
			{
				return "Translate2DH";
			}
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "dx", "dy" };
			}
		}

		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
		{
			double[,] E = new double[,] { { 1, 0, Argument1 }, { 0, 1, Argument2 }, { 0, 0, 1 } };

			return new DoubleMatrix(E);
		}

		public override IElement EvaluateScalar(Complex Argument1, Complex Argument2, Variables Variables)
		{
			Complex[,] E = new Complex[,] { { 1, 0, Argument1 }, { 0, 1, Argument2 }, { 0, 0, 1 } };

			return new ComplexMatrix(E);
		}
	}
}
