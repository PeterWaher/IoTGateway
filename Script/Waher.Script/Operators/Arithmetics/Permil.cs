using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Permil operator.
	/// </summary>
	public class Permil : UnaryDoubleOperator, IDifferentiable
	{
		/// <summary>
		/// Permil operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Permil(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}
	
		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(double Operand)
		{
			return new DoubleNumber(Operand / 1000);
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (this.op is IDifferentiable Differentiable)
			{
				return new Permil(
					Differentiable.Differentiate(VariableName, Variables),
					this.Start, this.Length, this.Expression);
			}
			else
				throw new ScriptRuntimeException("Operand not differentiable.", this);
		}
	}
}
