using System.Collections.Generic;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// PopFirst(v)
    /// </summary>
    public class PopFirst : FunctionOneVectorVariable
    {
        private readonly VariableReference reference;

        /// <summary>
        /// PopFirst(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public PopFirst(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
            this.reference = Argument as VariableReference;
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(PopFirst);

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            int c = Argument.Dimension;
            if (c == 0)
                return ObjectValue.Null;

            IElement Result = Argument.GetElement(0);

            if (!(this.reference is null))
            {
                ICollection<IElement> Elements = Argument.VectorElements;

                if (!Elements.Remove(Result))
                {
					ChunkedList<IElement> Elements2 = new ChunkedList<IElement>();
                    bool First = true;

                    foreach (IElement E in Elements)
                    {
                        if (First)
                            First = false;
                        else
                            Elements2.Add(E);
                    }

					Variables[this.reference.VariableName] = Argument.Encapsulate(Elements2, this);
				}
                else
                    Variables[this.reference.VariableName] = Argument.Encapsulate(Elements, this);
            }

            return Result;
        }

    }
}
