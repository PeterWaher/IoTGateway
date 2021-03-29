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
    /// IF operator.
    /// </summary>
    public class If : TernaryOperator
    {
        /// <summary>
        /// IF operator.
        /// </summary>
        /// <param name="Condition">Required condition.</param>
        /// <param name="IfTrue">Required statement, if true.</param>
        /// <param name="IfFalse">Optional statement, if false.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public If(ScriptNode Condition, ScriptNode IfTrue, ScriptNode IfFalse, int Start, int Length, Expression Expression)
            : base(Condition, IfTrue, IfFalse, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Condition = this.left.Evaluate(Variables);
            bool? b = ToBoolean(Condition);

            if (b.HasValue)
            {
                if (b.Value)
                    return this.middle.Evaluate(Variables);
                else if (this.right is null)
                    return ObjectValue.Null;
                else
                    return this.right.Evaluate(Variables);
            }

            IElement IfTrue = this.middle.Evaluate(Variables);
            IElement IfFalse = this.right is null ? ObjectValue.Null : this.right.Evaluate(Variables);

            return this.Evaluate(Condition, IfTrue, IfFalse);
        }

        private IElement Evaluate(IElement Condition, IElement IfTrue, IElement IfFalse)
        {
            if (Condition.IsScalar)
            {
                bool? b = ToBoolean(Condition);

                if (b.HasValue)
                {
                    if (b.Value)
                        return IfTrue;
                    else
                        return IfFalse;
                }

                throw new ScriptRuntimeException("Conditions must be boolean values, canonically correspond to boolean values, or encapsulate boolean values.", this);
            }
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();
                ICollection<IElement> ConditionElements = Condition.ChildElements;
                ICollection<IElement> IfTrueElements = IfTrue.IsScalar ? null : IfTrue.ChildElements;
                ICollection<IElement> IfFalseElements = IfFalse.IsScalar ? null : IfFalse.ChildElements;
                int c = ConditionElements.Count;

                if ((!(IfTrueElements is null) && IfTrueElements.Count != c) ||
                    (!(IfFalseElements is null) && IfFalseElements.Count != c))
                {
                    throw new ScriptRuntimeException("Operand dimension mismatch.", this);
                }

                IEnumerator<IElement> e1 = ConditionElements.GetEnumerator();
                IEnumerator<IElement> e2 = IfTrueElements?.GetEnumerator();
                IEnumerator<IElement> e3 = IfFalseElements?.GetEnumerator();

                while (e1.MoveNext() && (e2 is null || e2.MoveNext()) && (e3 is null || e3.MoveNext()))
                    Elements.AddLast(this.Evaluate(e1.Current, e2 is null ? IfTrue : e2.Current, e3 is null ? IfFalse : e3.Current));

                return Condition.Encapsulate(Elements, this);
            }
        }

    }
}
