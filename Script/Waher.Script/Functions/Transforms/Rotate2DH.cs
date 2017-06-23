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
	/// <summary>
	/// Creates a rotation matrix in 2-dimentional homogenous coordinates.
	/// </summary>
	public class Rotate2DH : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a rotation matrix in 2-dimentional homogenous coordinates.
		/// </summary>
		/// <param name="Angle">Angle of rotation.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Rotate2DH(ScriptNode Angle, int Start, int Length, Expression Expression)
			: base(Angle, Start, Length, Expression)
		{
		}

		public override string FunctionName
		{
			get
			{
				return "Rotate2DH";
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
			double[,] E = new double[,] { { C, S, 0 }, { -S, C, 0 }, { 0, 0, 1 } };

			return new DoubleMatrix(E);
		}
	}
}
