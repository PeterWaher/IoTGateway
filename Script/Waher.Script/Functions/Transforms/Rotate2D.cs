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
	/// Creates a rotation matrix in 2-dimentional space.
	/// </summary>
	public class Rotate2D : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a rotation matrix in 2-dimentional space.
		/// </summary>
		/// <param name="Angle">Angle of rotation.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Rotate2D(ScriptNode Angle, int Start, int Length, Expression Expression)
			: base(Angle, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Rotate2D";
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "angle" };
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			double S = Math.Sin(Argument);
			double C = Math.Cos(Argument);
			double[,] E = new double[,] { { C, S }, { -S, C } };

			return new DoubleMatrix(E);
		}
	}
}
