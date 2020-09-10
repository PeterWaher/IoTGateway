using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Statistics.Functions.Analytic
{
	/// <summary>
	/// Error function erf(z)
	/// </summary>
	public class Erf : FunctionOneScalarVariable
	{
		/// <summary>
		/// Error function erf(z)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Erf(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "erf";

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			double mz2 = -Argument * Argument;
			double Sum = Argument;
			double Product = 1;
			double Term;
			int n = 0;

			do
			{
				n++;
				Product *= mz2 / n;
				Term = Argument * Product / (2 * n + 1);
				Sum += Term;
			}
			while (Math.Abs(Term) > 1e-10);

			Sum *= c;

			return new DoubleNumber(Sum);
		}

		private static readonly double c = 2 / Math.Sqrt(Math.PI);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument, Variables Variables)
		{
			Complex mz2 = -Argument * Argument;
			Complex Sum = Argument;
			Complex Product = 1;
			Complex Term;
			int n = 0;

			do
			{
				n++;
				Product *= mz2 / n;
				Term = Argument * Product / (2 * n + 1);
				Sum += Term;
			}
			while (Complex.Abs(Term) > 1e-10);

			Sum *= c;

			return new ComplexNumber(Sum);
		}
	}
}
