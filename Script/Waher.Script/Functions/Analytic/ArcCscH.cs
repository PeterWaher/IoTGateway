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
	/// ArcCscH(x)
	/// </summary>
	public class ArcCscH : FunctionOneScalarVariable, IDifferentiable
	{
		/// <summary>
		/// ArcCscH(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ArcCscH(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "arccsch"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get { return new string[] { "acsch" }; }
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
					new Invert(
						new Negate(
							new Multiply(
								new Scalar.Abs(this.Argument, Start, Len, Expression),
								new Sqrt(
									new Add(
										new Square(this.Argument, Start, Len, Expression),
										new ConstantElement(DoubleNumber.OneElement, Start, Len, Expression),
										Start, Len, Expression),
									Start, Len, Expression),
								Start, Len, Expression),
							Start, Len, Expression),
						Start, Len, Expression));
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
			double d = 1 / Argument;
			return new DoubleNumber(Math.Log(d + Math.Sqrt(d * d + 1)));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument, Variables Variables)
		{
			Complex d = Complex.Reciprocal(Argument);
			return new ComplexNumber(Complex.Log(d + Complex.Sqrt(d * d + Complex.One)));
		}
	}
}
