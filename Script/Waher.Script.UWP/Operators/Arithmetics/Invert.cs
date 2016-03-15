using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Inversion operator.
	/// </summary>
	public class Invert : UnaryOperator 
	{
		/// <summary>
		/// Inversion operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Invert(ScriptNode Operand, int Start, int Length)
			: base(Operand, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Operand = this.op.Evaluate(Variables);
            return this.Evaluate(Operand);
		}

        private IElement Evaluate(IElement Element)
        {
            IRingElement E = Operand as IRingElement;
            if (E != null)
            {
                E = E.Invert();
                if (E == null)
                    throw new ScriptRuntimeException("Operand not invertible.", this);
                else
                    return E;
            }
            else if (E.IsScalar)
                    throw new ScriptRuntimeException("Operand not invertible.", this);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E2 in E.ChildElements)
                    Elements.AddLast(this.Evaluate(E2));

                return E.Encapsulate(Elements, this);
            }
        }
    }
}
