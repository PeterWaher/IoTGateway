using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Operators.Sets
{
    /// <summary>
    /// Creates a set.
    /// </summary>
    public class SetDefinition : ElementList
    {
        /// <summary>
        /// Creates a set.
        /// </summary>
        /// <param name="Rows">Row vectors.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public SetDefinition(ScriptNode[] Elements, int Start, int Length)
            : base(Elements, Start, Length)
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

            foreach (ScriptNode N in this.Elements)
                Elements.AddLast(N.Evaluate(Variables));

            return Encapsulate(Elements, this);
        }

        /// <summary>
        /// Encapsulates the elements of a set.
        /// </summary>
        /// <param name="Elements">Set elements.</param>
        /// <param name="Node">Script node from where the encapsulation is done.</param>
        /// <returns>Encapsulated set.</returns>
        public static IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
        {
            return new FiniteSet(Elements);
        }

    }
}