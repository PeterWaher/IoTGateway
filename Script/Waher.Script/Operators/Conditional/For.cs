using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Conditional
{
    /// <summary>
    /// FOR operator.
    /// </summary>
    public class For : QuaternaryOperator
    {
        private readonly string variableName;

        /// <summary>
        /// FOR operator.
        /// </summary>
		/// <param name="VariableName">Name of loop variable.</param>
        /// <param name="From">Required From statement.</param>
        /// <param name="To">Required To statement.</param>
        /// <param name="Step">Optional Step statement.</param>
		/// <param name="Statement">Loop statement to evaluate.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public For(string VariableName, ScriptNode From, ScriptNode To, ScriptNode Step, ScriptNode Statement, int Start, int Length, Expression Expression)
            : base(From, To, Step, Statement, Start, Length, Expression)
        {
            this.variableName = VariableName;
        }

        /// <summary>
        /// Variable Name.
        /// </summary>
        public string VariableName
        {
            get { return this.variableName; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            if (!(this.left.Evaluate(Variables) is ICommutativeRingWithIdentityElement From))
                throw new ScriptRuntimeException("Invalid range.", this);

            if (!(this.middle.Evaluate(Variables) is ICommutativeRingWithIdentityElement To))
                throw new ScriptRuntimeException("Invalid range.", this);

            if (!(From.AssociatedSet is IOrderedSet S))
                throw new ScriptRuntimeException("Cannot compare range.", this);

            IElement Step, Last;
            int Direction = S.Compare(From, To);
            bool Done;

            if (!(this.middle2 is null))
            {
                Step = this.middle2.Evaluate(Variables);

                if (Direction < 0)
                {
                    if (S.Compare(Step, From.Zero) <= 0)
                        throw new ScriptRuntimeException("Invalid step size for corresponding range.", this);
                }
                else if (Direction > 0)
                {
                    if (S.Compare(Step, From.Zero) >= 0)
                        throw new ScriptRuntimeException("Invalid step size for corresponding range.", this);
                }
            }
            else
            {
                if (Direction <= 0)
                    Step = From.One;
                else
                    Step = From.One.Negate();
            }

            do
            {
                Variables[this.variableName] = From;
                Last = this.right.Evaluate(Variables);

                if (Direction == 0)
                    Done = true;
                else
                {
                    From = Operators.Arithmetics.Add.EvaluateAddition(From, Step, this) as ICommutativeRingWithIdentityElement;
                    if (From is null)
                        throw new ScriptRuntimeException("Invalid step size.", this);

                    if (Direction > 0)
                        Done = S.Compare(From, To) < 0;
                    else
                        Done = S.Compare(From, To) > 0;
                }
            }
            while (!Done);

            return Last;
        }
    }
}
