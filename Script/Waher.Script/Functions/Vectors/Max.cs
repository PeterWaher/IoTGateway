using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Max(v)
    /// </summary>
    public class Max : FunctionOneVectorVariable
    {
        /// <summary>
        /// Max(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Max(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "max"; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            IElement Result = null;
            IOrderedSet S = null;

            foreach (IElement E in Argument.ChildElements)
            {
                if (Result == null || S.Compare(Result, E) < 0)
                {
                    Result = E;
                    S = Result.AssociatedSet as IOrderedSet;
                    if (S == null)
                        throw new ScriptRuntimeException("Cannot compare operands.", this);
                }
            }

            if (Result == null)
                return ObjectValue.Null;
            else
                return Result;
        }

    }
}
