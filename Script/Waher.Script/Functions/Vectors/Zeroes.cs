using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Creates a vector containing only ones.
	/// </summary>
	public class Zeroes : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a vector containing only ones.
		/// </summary>
		/// <param name="Dimension">Dimension of vector.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Zeroes(ScriptNode Dimension, int Start, int Length, Expression Expression)
			: base(Dimension, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Zeroes";
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
			int N = (int)Argument;

			if (N != Argument || N < 0)
				throw new ScriptRuntimeException("Dimension must be a non-negative integer.", this);

			double[] E = new double[N];

			return new DoubleVector(E);
		}
	}
}
