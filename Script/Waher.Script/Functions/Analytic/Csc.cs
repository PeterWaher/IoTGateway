using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Analytic
{
	/// <summary>
	/// Csc(x)
	/// </summary>
	public class Csc : FunctionOneScalarVariable, IDifferentiable
	{
		/// <summary>
		/// Csc(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Csc(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "csc"; }
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (VariableName == this.DefaultVariableName)
			{
				int Start = this.Start;
				int Len = this.Length;
				Expression Exp = this.Expression;

				return this.DifferentiationChainRule(VariableName, Variables, this.Argument,
					new Negate(
						new Multiply(
							new Csc(this.Argument, Start, Len, Exp),
							new Cot(this.Argument, Start, Len, Exp),
							Start, Len, Exp),
						Start, Len, Exp));
			}
			else
				return new ConstantElement(DoubleNumber.ZeroElement, this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			return new DoubleNumber(1.0 / Math.Sin(Argument));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument, Variables Variables)
		{
			return new ComplexNumber(Complex.Reciprocal(Complex.Sin(Argument)));
		}
	}
}
