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
	/// Creates a scaling matrix in 2-dimentional space.
	/// </summary>
	public class Scale2D : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Creates a scaling matrix in 2-dimentional space.
		/// </summary>
		/// <param name="Sx">Scaling factor along x-axis.</param>
		/// <param name="Sy">Scaling factor along y-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Scale2D(ScriptNode Sx, ScriptNode Sy, int Start, int Length, Expression Expression)
			: base(Sx, Sy, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Scale2D";
			}
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "sx", "sy" };
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
		{
			double[,] E = new double[,] { { Argument1, 0 }, { 0, Argument2 } };

			return new DoubleMatrix(E);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument1, Complex Argument2, Variables Variables)
		{
			Complex[,] E = new Complex[,] { { Argument1, 0 }, { 0, Argument2 } };

			return new ComplexMatrix(E);
		}
	}
}
