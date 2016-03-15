using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Left(s,n)
    /// </summary>
    public class Left : FunctionTwoScalarVariables
    {
        /// <summary>
        /// Left(x,n)
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Left(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length)
            : base(Argument1, Argument2, Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "left"; }
        }
        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
        {
            StringValue S = Argument1 as StringValue;
            if (S == null)
                throw new ScriptRuntimeException("Expected string in first argument.", this);

            DoubleNumber N = Argument2 as DoubleNumber;
            double d;

            if (N == null || (d = N.Value) < 0 || d > int.MaxValue || d != Math.Truncate(d))
                throw new ScriptRuntimeException("Expected nonnegative integer in second argument.", this);

            int i = (int)d;
            string s = S.Value;

            if (i > s.Length)
                return S;
            else
                return new StringValue(s.Substring(0, i));
        }

    }
}
