﻿using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators.Conditional;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Creates a set using a DO-WHILE statement.
	/// </summary>
	public class SetDoWhileDefinition : VectorDoWhileDefinition
    {
		/// <summary>
		/// Creates a set using a DO-WHILE statement.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SetDoWhileDefinition(DoWhile Elements, int Start, int Length, Expression Expression)
            : base(Elements, Start, Length, Expression)
        {
		}

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements</param>
        /// <returns>Encapsulated elements.</returns>
        protected override IElement Encapsulate(ChunkedList<IElement> Elements)
        {
            return SetDefinition.Encapsulate(Elements);
        }

    }
}
