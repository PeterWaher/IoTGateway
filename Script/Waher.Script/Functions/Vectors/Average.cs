using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Average(v), Avg(v)
    /// </summary>
    public class Average : FunctionOneVectorVariable
    {
        /// <summary>
        /// Average(v), Avg(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Average(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "average"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "avg" }; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            return EvaluateAverage(Argument, this);
        }

        /// <summary>
        /// Calculates the average of the elements of a vector.
        /// </summary>
        /// <param name="Vector">Vector</param>
        /// <param name="Node">Node performing evaluation.</param>
        /// <returns>Average of elements.</returns>
        public static IElement EvaluateAverage(IVector Vector, ScriptNode Node)
        {
            // TODO: Optimized results for double and complex vectors.

            IElement Result = Vectors.Sum.EvaluateSum(Vector, Node);
            int n = Vector.Dimension;

            if (Result == null)
                return ObjectValue.Null;
            else
            {
                IRingElement RE = Result as IRingElement;
                IRingElement Avg;

                if (RE != null && (Avg = RE.MultiplyRight(new DoubleNumber(1.0 / n))) != null)
                    return Avg;
                else
                    return Operators.Arithmetics.Divide.EvaluateDivision(Result, new DoubleNumber(n), Node);
            }
        }

    }
}
