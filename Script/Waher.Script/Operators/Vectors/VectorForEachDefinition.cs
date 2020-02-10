using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector using a FOREACH statement.
	/// </summary>
	public class VectorForEachDefinition : BinaryOperator
	{
        private readonly string variableName;

		/// <summary>
		/// Creates a vector using a FOREACH statement.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorForEachDefinition(ForEach Elements, int Start, int Length, Expression Expression)
			: base(Elements.LeftOperand, Elements.RightOperand, Start, Length, Expression)
		{
            this.variableName = Elements.VariableName;
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
            IElement S = this.left.Evaluate(Variables);
            if (!(S is ICollection<IElement> Elements))
            {
                if (S is IVector Vector)
                    Elements = Vector.VectorElements;
                else if (!S.IsScalar)
                    Elements = S.ChildElements;
                else
                    Elements = new IElement[] { S };
            }

            LinkedList<IElement> Elements2 = new LinkedList<IElement>();

            foreach (IElement Element in Elements)
            {
                Variables[this.variableName] = Element;
                Elements2.AddLast(this.right.Evaluate(Variables));
            }

            return this.Encapsulate(Elements2);
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
