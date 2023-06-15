using System;
using System.Threading.Tasks;
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

			if (Op.AssociatedObjectValue is double d)
				return this.Evaluate(d);
			else
				return this.Evaluate(Op, Variables);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return base.Evaluate(Variables);

			IElement Op = await this.op.EvaluateAsync(Variables);

			if (Op.AssociatedObjectValue is double d)
				return await this.EvaluateAsync(d);
			else
				return await this.EvaluateAsync(Op, Variables);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand, Variables Variables)
		{
			object Obj = Operand.AssociatedObjectValue;

			if (Obj is double d)
				return this.Evaluate(d);
			else if (Expression.TryConvert(Obj, out d))
				return this.Evaluate(d);
			else
				throw new ScriptRuntimeException("Scalar operands must be double values or physical magnitudes.", this);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override async Task<IElement> EvaluateScalarAsync(IElement Operand, Variables Variables)
		{
			object Obj = Operand.AssociatedObjectValue;

			if (Obj is double d)
				return await this.EvaluateAsync(d);
			else if (Expression.TryConvert(Obj, out d))
				return await this.EvaluateAsync(d);
			else
				throw new ScriptRuntimeException("Scalar operands must be double values or physical magnitudes.", this);
		}

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(double Operand);

		/// <summary>
		/// Evaluates the double operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateAsync(double Operand)
		{
			return Task.FromResult<IElement>(this.Evaluate(Operand));
		}
	}
}
