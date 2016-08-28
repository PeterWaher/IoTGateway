using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Median(v)
    /// </summary>
    public class Median : FunctionOneVectorVariable
    {
        /// <summary>
        /// Median(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Median(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "median"; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            double[] v = Argument.Values;

            if (v.Length == 0)
                return ObjectValue.Null;

            return new DoubleNumber(CalcMedian(Argument.Values, this));
        }

        /// <summary>
        /// Returns the median value.
        /// </summary>
        /// <param name="Values">Set of values. Must not be empty.</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Median value.</returns>
        public static double CalcMedian(double[] Values, ScriptNode Node)
        {
            int c = Values.Length;

            if (c == 0)
                throw new ScriptRuntimeException("Empty set of values.", Node);

            Array.Sort<double>(Values);

            return Values[c / 2];
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            ICollection<IElement> Elements = Argument.ChildElements;
            int c = 0;

            if (c == 0)
                throw new ScriptRuntimeException("Empty set of values.", this);

            IElement[] A = new IElement[Elements.Count];
            Elements.CopyTo(A, 0);

            Array.Sort<IElement>(A, this.Compare);

            return A[c / 2];
        }

        private int Compare(IElement e1, IElement e2)
        {
            IOrderedSet S1 = e1.AssociatedSet as IOrderedSet;
            IOrderedSet S2 = e2.AssociatedSet as IOrderedSet;

            if (S1 == null || S2 == null || S1 != S2)
                throw new ScriptRuntimeException("Cannot order elements.", this);

            return S1.Compare(e1, e2);
        }

    }
}
