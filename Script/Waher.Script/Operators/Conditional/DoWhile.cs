using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// DO-WHILE operator.
	/// </summary>
	public class DoWhile : BinaryOperator
	{
		/// <summary>
		/// DO-WHILE operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="Condition">Condition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DoWhile(ScriptNode Statement, ScriptNode Condition, int Start, int Length)
			: base(Statement, Condition, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Last;
            BooleanValue Condition;

            do
            {
                Last = this.left.Evaluate(Variables);

                Condition = this.right.Evaluate(Variables) as BooleanValue;
                if (Condition == null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }
            while (Condition.Value);

            return Last;
		}
	}
}
