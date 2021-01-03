using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for unary boolean operators.
	/// </summary>
	public abstract class UnaryBooleanOperator : UnaryScalarOperator
	{
		/// <summary>
		/// Base class for binary boolean operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UnaryBooleanOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Op = this.op.Evaluate(Variables);

			if (Op is BooleanValue BOp)
				return this.Evaluate(BOp.Value);
			else
				return this.Evaluate(Op, Variables);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand, Variables Variables)
		{
			if (Operand is BooleanValue BOp)
				return this.Evaluate(BOp.Value);
			else
				throw new ScriptRuntimeException("Scalar operands must be boolean values.", this);
		}

		/// <summary>
		/// Evaluates the boolean operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(bool Operand);

	}
}
