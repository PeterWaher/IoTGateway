using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Conditional;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Creates a set using a FOREACH statement.
	/// </summary>
	public class SetForEachDefinition : VectorForEachDefinition
    {
        /// <summary>
        /// Creates a set using a FOREACH statement.
        /// </summary>
        /// <param name="Rows">Row vectors.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public SetForEachDefinition(ForEach Elements, int Start, int Length)
            : base(Elements, Start, Length)
        {
        }

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements</param>
        /// <returns>Encapsulated elements.</returns>
        protected override IElement Encapsulate(LinkedList<IElement> Elements)
        {
            return SetDefinition.Encapsulate(Elements, this);
        }

    }
}
