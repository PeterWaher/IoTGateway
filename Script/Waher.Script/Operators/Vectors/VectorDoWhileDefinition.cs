using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector using a DO-WHILE statement.
	/// </summary>
	public class VectorDoWhileDefinition : BinaryOperator
	{
		/// <summary>
		/// Creates a vector using a DO-WHILE statement.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorDoWhileDefinition(DoWhile Elements, int Start, int Length, Expression Expression)
			: base(Elements.LeftOperand, Elements.RightOperand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            LinkedList<IElement> Elements = new LinkedList<IElement>();
            BooleanValue Condition;

            do
            {
                Elements.AddLast(this.left.Evaluate(Variables));

                Condition = this.right.Evaluate(Variables) as BooleanValue;
                if (Condition == null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }
            while (Condition.Value);

            return this.Encapsulate(Elements);
        }

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements to encapsulate.</param>
        /// <returns>Encapsulated element.</returns>
        protected virtual IElement Encapsulate(LinkedList<IElement> Elements)
        {
            return VectorDefinition.Encapsulate(Elements, false, this);
        }

    }
}
