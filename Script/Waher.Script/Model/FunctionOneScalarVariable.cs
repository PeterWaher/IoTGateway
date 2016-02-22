using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one scalar variable.
    /// </summary>
    public abstract class FunctionOneScalarVariable : FunctionOneVariable 
	{
        /// <summary>
        /// Base class for funcions of one scalar variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionOneScalarVariable(ScriptNode Argument, int Start, int Length)
			: base(Argument, Start, Length)
		{
		}

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            if (Argument.IsScalar)
                return this.EvaluateScalar(Argument, Variables);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();

                foreach (IElement E in Argument.ChildElements)
                    Elements.AddLast(this.Evaluate(E, Variables));

                return Argument.Encapsulate(Elements, this);
            }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement EvaluateScalar(IElement Argument, Variables Variables);

    }
}
