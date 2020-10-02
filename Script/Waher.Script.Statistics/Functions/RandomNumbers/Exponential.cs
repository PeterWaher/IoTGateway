using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Statistics.Functions.RandomNumbers
{
    /// <summary>
    /// Generates a random number using the exponential distribution.
    /// </summary>
    public class Exponential : FunctionMultiVariate
    {
        private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };
        private static readonly ArgumentType[] argumentTypes1Parameters = new ArgumentType[] { ArgumentType.Scalar };
        private static readonly ArgumentType[] argumentTypes0Parameters = new ArgumentType[] { };

		/// <summary>
		/// Generates a random number using the exponential distribution.
		/// </summary>
		/// <param name="Mean">Mean of distribution.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Exponential(ScriptNode Mean, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Mean, N }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the exponential distribution.
		/// </summary>
		/// <param name="Mean">Mean of distribution.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Exponential(ScriptNode Mean, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Mean }, argumentTypes1Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the exponential distribution.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Exponential(int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { }, argumentTypes0Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
        {
            get { return "exponential"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "mean", "N" }; }
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
                    double Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);

                    return new DoubleNumber(NextDouble() * Mean);

                case 2:
                default:
                    Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    double d = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
                    int N = (int)Math.Round(d);
                    if (N < 0 || N != d)
                        throw new ScriptRuntimeException("N must be a non-negative integer.", this);

                    double[] v = new double[N];

                    for (int i = 0; i < N; i++)
                        v[i] = NextDouble() * Mean;

                    return new DoubleVector(v);
            }
        }

		/// <summary>
		/// Gets a exponentially distributed random value. Distribution has a mean of 0 and standard deviation of 1.
		/// </summary>
		/// <returns>Random value.</returns>
		public static double NextDouble()
        {
			return -Math.Log(Uniform.NextDouble());
		}

	}
}
