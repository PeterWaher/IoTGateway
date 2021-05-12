using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for unary double operators.
	/// </summary>
	public abstract class UnaryDoubleOperator : UnaryScalarOperator
	{
		/// <summary>
		/// Base class for binary double operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UnaryDoubleOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
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

			if (Op is DoubleNumber DOp)
				return this.Evaluate(DOp.Value);
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
			if (Operand is DoubleNumber DOp)
				return this.Evaluate(DOp.Value);
			else if (Expression.TryConvert<double>(Operand.AssociatedObjectValue, out double d))
				return this.Evaluate(d);
			else
				throw new ScriptRuntimeException("Scalar operands must be double values or physical magnitudes.", this);
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(double Operand);

	}
}
