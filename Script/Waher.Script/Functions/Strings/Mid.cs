using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Mid(s,start,len)
    /// </summary>
    public class Mid : FunctionMultiVariate
    {
        /// <summary>
        /// Mid(s,start,len)
        /// </summary>
        /// <param name="String">String.</param>
        /// <param name="StartPos">Starting position in string.</param>
		/// <param name="NrCharacters">Number of characters to extract.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Mid(ScriptNode String, ScriptNode StartPos, ScriptNode NrCharacters, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { String, StartPos, NrCharacters },
                  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "mid"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "s", "pos", "len" }; }
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            StringValue S = Arguments[0] as StringValue;
            if (S == null)
                throw new ScriptRuntimeException("Expected string in first argument.", this);

            DoubleNumber Start = Arguments[1] as DoubleNumber;
            DoubleNumber Len = Arguments[2] as DoubleNumber;
            double d;

            if (Start == null || (d = Start.Value) < 0 || d > int.MaxValue || d != Math.Truncate(d))
                throw new ScriptRuntimeException("Expected nonnegative integer in second argument.", this);

            int pos = (int)d;

            if (Len == null || (d = Start.Value) < 0 || d > int.MaxValue || d != Math.Truncate(d))
                throw new ScriptRuntimeException("Expected nonnegative integer in third argument.", this);

            string s = S.Value;
            int n = (int)d;
            int len = s.Length;

            if (pos > len)
                return StringValue.Empty;
            else if (pos + n >= len)
                return new StringValue(s.Substring(pos));
            else
                return new StringValue(s.Substring(pos, n));
        }

    }
}
