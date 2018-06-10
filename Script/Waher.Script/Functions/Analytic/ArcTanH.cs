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
    /// ArcTanH(x)
    /// </summary>
    public class ArcTanH : FunctionOneScalarVariable, IDifferentiable
    {
        /// <summary>
        /// ArcTanH(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public ArcTanH(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "arctanh"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "atanh" }; }
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
				if (this.Argument is IDifferentiable Differentiable)
				{
					int Start = this.Start;
					int Len = this.Length;
					Expression Exp = this.Expression;

					return new Multiply(
						new Invert(
							new Subtract(
								new ConstantElement(DoubleNumber.OneElement, Start, Len, Expression),
								new Square(this.Argument, Start, Len, Expression),
								Start, Len, Expression),
							Start, Len, Expression),
						Differentiable.Differentiate(VariableName, Variables), Start, Len, Expression);
				}
				else
					throw new ScriptRuntimeException("Argument not differentiable.", this);
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
            return new DoubleNumber(0.5 * (Math.Log(1 + Argument) - Math.Log(1 - Argument)));
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(Complex Argument, Variables Variables)
        {
            return new ComplexNumber(0.5 * (Complex.Log(Complex.One + Argument) - Complex.Log(Complex.One - Argument)));
        }
    }
}
