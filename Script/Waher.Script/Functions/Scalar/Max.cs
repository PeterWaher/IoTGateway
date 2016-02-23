using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Max(x,y)
    /// </summary>
    public class Max : FunctionTwoScalarVariables
    {
        /// <summary>
        /// Max(x,y)
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Max(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length)
            : base(Argument1, Argument2, Start, Length)
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
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
        {
            IOrderedSet S = Argument1.AssociatedSet as IOrderedSet;
            if (S == null)
                throw new ScriptRuntimeException("Unable to compare elements.", this);

            if (S.Compare(Argument1, Argument2) > 0)
                return Argument1;
            else
                return Argument2;
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
        {
            return new DoubleNumber(Math.Max(Argument1, Argument2));
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
        {
            return new StringValue(string.Compare(Argument1, Argument2) > 0 ? Argument1 : Argument2);
        }

    }
}
