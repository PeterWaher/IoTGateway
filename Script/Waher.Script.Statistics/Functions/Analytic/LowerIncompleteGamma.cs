using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Script.Statistics.Functions.Analytic
{
	/// <summary>
	/// Lower Incomplete Gamma function γ(a,z)
	/// </summary>
	public class LowerIncompleteGamma : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Lower Incomplete Gamma function γ(a,z)
		/// </summary>
		/// <param name="Argument1">a</param>
		/// <param name="Argument2">z</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LowerIncompleteGamma(ScriptNode Argument1, ScriptNode Argument2, 
			int Start, int Length, Expression Expression)
			: base(Argument1, Argument2, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "γ";

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "lgamma" };

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "a", "z" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
		{
			return new DoubleNumber(StatMath.γ(Argument1, Argument2));
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument1, Complex Argument2, Variables Variables)
		{
			return new ComplexNumber(StatMath.γ(Argument1, Argument2));
		}
	}
}
