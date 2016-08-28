using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for unary scalar operators.
	/// </summary>
	public abstract class UnaryScalarOperator : UnaryOperator
	{
		/// <summary>
		/// Base class for unary scalar operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public UnaryScalarOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
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

			return this.Evaluate(Operand, Variables);
		}

        /// <summary>
        /// Evaluates the operator.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public virtual IElement Evaluate(IElement Operand, Variables Variables)
		{
			if (Operand.IsScalar)
				return this.EvaluateScalar(Operand, Variables);
			else
			{
				LinkedList<IElement> Result = new LinkedList<IElement>();

				foreach (IElement Child in Operand.ChildElements)
					Result.AddLast(this.Evaluate(Child, Variables));

				return Operand.Encapsulate(Result, this);
			}
		}

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public abstract IElement EvaluateScalar(IElement Operand, Variables Variables);
	}
}
