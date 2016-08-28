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
    /// WHILE-DO operator.
    /// </summary>
    public class WhileDo : BinaryOperator
    {
        /// <summary>
        /// WHILE-DO operator.
        /// </summary>
        /// <param name="Condition">Condition.</param>
        /// <param name="Statement">Statement.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public WhileDo(ScriptNode Condition, ScriptNode Statement, int Start, int Length, Expression Expression)
            : base(Condition, Statement, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Last = null;
            BooleanValue Condition;

            Condition = this.left.Evaluate(Variables) as BooleanValue;
            if (Condition == null)
                throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);

            while (Condition.Value)
            {
                Last = this.right.Evaluate(Variables);

                Condition = this.left.Evaluate(Variables) as BooleanValue;
                if (Condition == null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }

            if (Last == null)
                return ObjectValue.Null;
            else
                return Last;
        }
    }
}
