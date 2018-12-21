using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// To-Set operator.
	/// </summary>
	public class ToSet : NullCheckUnaryOperator
	{
		/// <summary>
		/// To-Set operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToSet(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, NullCheck, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement E = this.op.Evaluate(Variables);

            if (E is ISet)
                return E;

			if (E is IVector V)
				return SetDefinition.Encapsulate(V.VectorElements, this);

			if (this.nullCheck && E.AssociatedObjectValue is null)
				return E;

			return SetDefinition.Encapsulate(new IElement[] { E }, this);
        }
    }
}
