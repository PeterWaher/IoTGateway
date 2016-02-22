using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Sum(v)
    /// </summary>
    public class Sum : FunctionOneVectorVariable
    {
        /// <summary>
        /// Sum(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Sum(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "sum"; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            return EvaluateSum(Argument, this);
        }

        /// <summary>
        /// Sums the elements of a vector.
        /// </summary>
        /// <param name="Vector">Vector</param>
        /// <param name="Node">Node performing evaluation.</param>
        /// <returns>Sum of elements.</returns>
        public static IElement EvaluateSum(IVector Vector, ScriptNode Node)
        {
            // TODO: Optimized results for double and complex vectors.

            ISemiGroupElement Result = null;
            ISemiGroupElement SE;
            ISemiGroupElement Sum;

            foreach (IElement E in Vector.ChildElements)
            {
                SE = E as ISemiGroupElement;
                if (SE == null)
                    throw new ScriptRuntimeException("Elements not addable.", Node);

                if (Result == null)
                    Result = SE;
                else
                {
                    Sum = Result.AddRight(SE);
                    if (Sum == null)
                        Sum = (ISemiGroupElement)Operators.Arithmetics.Add.EvaluateAddition(Result, SE, Node);

                    Result = Sum;
                }
            }

            if (Result == null)
                return ObjectValue.Null;
            else
                return Result;
        }

    }
}
