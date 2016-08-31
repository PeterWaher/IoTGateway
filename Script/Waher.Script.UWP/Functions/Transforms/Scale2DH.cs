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
	public class Scale2DH : FunctionTwoScalarVariables
	{
		public Scale2DH(ScriptNode Sx, ScriptNode Sy, int Start, int Length, Expression Expression)
			: base(Sx, Sy, Start, Length, Expression)
		{
		}

		public override string FunctionName
		{
			get
			{
				return "Scale2DH";
			}
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "sx", "sy" };
			}
		}

		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
		{
			double[,] E = new double[,] { { Argument1, 0, 0 }, { 0, Argument2, 0 }, { 0, 0, 1 } };

			return new DoubleMatrix(E);
		}

		public override IElement EvaluateScalar(Complex Argument1, Complex Argument2, Variables Variables)
		{
			Complex[,] E = new Complex[,] { { Argument1, 0, 0 }, { 0, Argument2, 0 }, { 0, 0, 1 } };

			return new ComplexMatrix(E);
		}
	}
}
