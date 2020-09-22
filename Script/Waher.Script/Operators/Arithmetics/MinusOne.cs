using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Assignments.Pre;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// -1 operator.
	/// </summary>
	public class MinusOne : UnaryOperator, IDifferentiable
	{
		/// <summary>
		/// -1 operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MinusOne(ScriptNode Operand, int Start, int Length, Expression Expression)
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
			IElement Operand = this.op.Evaluate(Variables);

			return this.Evaluate(Operand);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <returns>Result</returns>
		public virtual IElement Evaluate(IElement Operand)
		{
			if (Operand is DoubleNumber DOp)
				return new DoubleNumber(DOp.Value - 1);
			else
				return PreDecrement.Decrement(Operand, this);
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (this.op is IDifferentiable D)
				return D.Differentiate(VariableName, Variables);
			else
				throw new ScriptRuntimeException("Argument not differentiable.", this);
		}

	}
}
