using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Statistics.Functions
{
    /// <summary>
    /// Generates a random number using the uniform distribution.
    /// </summary>
    public class Uniform : FunctionMultiVariate
    {
        private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
        private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };
        private static readonly ArgumentType[] argumentTypes1Parameters = new ArgumentType[] { ArgumentType.Scalar };
        private static readonly ArgumentType[] argumentTypes0Parameters = new ArgumentType[] { };
		private static RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private const double maxD = ((double)(1UL << 53));

        /// <summary>
        /// Generates a random number using the uniform distribution.
        /// </summary>
        /// <param name="Min">Minimum value.</param>
        /// <param name="Max">Maximum value.</param>
        /// <param name="N">Number of values to generate.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public Uniform(ScriptNode Min, ScriptNode Max, ScriptNode N, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Min, Max, N }, argumentTypes3Parameters, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Generates a random number using the uniform distribution.
        /// </summary>
        /// <param name="Min">Minimum value.</param>
        /// <param name="Max">Maximum value.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public Uniform(ScriptNode Min, ScriptNode Max, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Min, Max }, argumentTypes2Parameters, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Generates a random number using the uniform distribution.
        /// </summary>
        /// <param name="N">Number of values to generate.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public Uniform(ScriptNode N, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { N }, argumentTypes1Parameters, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Generates a random number using the uniform distribution.
        /// </summary>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression.</param>
        public Uniform(int Start, int Length, Expression Expression)
            : base(new ScriptNode[0], argumentTypes0Parameters, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "uniform"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "min", "max", "N" }; }
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            switch (Arguments.Length)
            {
                case 0:
                    return new DoubleNumber(NextDouble());

                case 1:
                    double d = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    int N = (int)Math.Round(d);
                    if (N < 0 || N != d)
                        throw new ScriptRuntimeException("N must be a non-negative integer.", this);

                    double[] v = new double[N];
                    int i;

                    for (i = 0; i < N; i++)
                        v[i] = NextDouble();

                    return new DoubleVector(v);

                case 2:
                    double Min = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    double Max = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

                    return new DoubleNumber(NextDouble() * (Max - Min) + Min);

                case 3:
                default:
                    Min = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    Max = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
                    d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
                    N = (int)Math.Round(d);
                    if (N < 0 || N != d)
                        throw new ScriptRuntimeException("N must be a non-negative integer.", this);

                    v = new double[N];

                    double Delta = Max - Min;

                    for (i = 0; i < N; i++)
                        v[i] = NextDouble() * Delta + Min;

                    return new DoubleVector(v);
            }
        }

        private static ulong NextULong()
        {
            byte[] Bin = new byte[8];

            lock (rnd)
            {
                rnd.GetBytes(Bin);
            }

            return BitConverter.ToUInt64(Bin, 0);
        }

        private static double NextDouble()
        {
            byte[] Bin = new byte[8];

            lock (rnd)
            {
                rnd.GetBytes(Bin);
            }

            ulong l = BitConverter.ToUInt64(Bin, 0);
            l >>= 11;

            return l / maxD;
        }

    }
}
