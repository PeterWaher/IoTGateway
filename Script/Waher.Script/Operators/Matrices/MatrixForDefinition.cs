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

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Creates a matrix using a FOR statement.
	/// </summary>
	public class MatrixForDefinition : VectorForDefinition
    {
		/// <summary>
		/// Creates a matrix using a FOR statement.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public MatrixForDefinition(For Elements, int Start, int Length, Expression Expression)
            : base(Elements, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements</param>
        /// <returns>Encapsulated elements.</returns>
        protected override IElement Encapsulate(LinkedList<IElement> Elements)
        {
            return MatrixDefinition.Encapsulate(Elements, this);
        }

    }
}
