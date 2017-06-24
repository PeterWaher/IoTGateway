using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Negation operator.
	/// </summary>
	public class Negate : UnaryDoubleOperator
	{
		/// <summary>
		/// Negation operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Negate(ScriptNode Operand, int Start, int Length, Expression Expression)
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
			return new DoubleNumber(-Operand);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand, Variables Variables)
		{
			if (Operand is IGroupElement GE)
				return GE.Negate();
			else
				throw new ScriptRuntimeException("Unable to negate objects of type " + Operand.GetType().FullName + ".", this);
		}

		/// <summary>
		/// Negates an object.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Negated version.</returns>
		public static IElement EvaluateNegation(IElement Operand)
		{
			if (Operand is IGroupElement E)
				return E.Negate();
			else
				throw new ScriptException("Operand cannot be negated.");
		}

	}
}
