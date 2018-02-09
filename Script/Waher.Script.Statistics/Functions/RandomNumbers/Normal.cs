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
    /// Generates a random number using the normal distribution.
    /// </summary>
    public class Normal : FunctionMultiVariate
    {
        private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
        private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };
        private static readonly ArgumentType[] argumentTypes1Parameters = new ArgumentType[] { ArgumentType.Scalar };
        private static readonly ArgumentType[] argumentTypes0Parameters = new ArgumentType[] { };

		/// <summary>
		/// Generates a random number using the normal distribution.
		/// </summary>
		/// <param name="Mean">Mean of distribution.</param>
		/// <param name="StdDev">Standard deviation of distribution.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Normal(ScriptNode Mean, ScriptNode StdDev, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Mean, StdDev, N }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the normal distribution.
		/// </summary>
		/// <param name="Mean">Mean of distribution.</param>
		/// <param name="StdDev">Standard deviation of distribution.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Normal(ScriptNode Mean, ScriptNode StdDev, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Mean, StdDev }, argumentTypes2Parameters, Start, Length, Expression)
        {
        }

		/// <summary>
		/// Generates a random number using the normal distribution.
		/// </summary>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Normal(ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N }, argumentTypes1Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the normal distribution.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Normal(int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { }, argumentTypes0Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
        {
            get { return "normal"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "mean", "stddev", "N" }; }
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
                    double Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    double StdDev = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

                    return new DoubleNumber(NextDouble() * StdDev + Mean);

                case 3:
                default:
                    Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
                    StdDev = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
                    d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
                    N = (int)Math.Round(d);
                    if (N < 0 || N != d)
                        throw new ScriptRuntimeException("N must be a non-negative integer.", this);

                    v = new double[N];

                    for (i = 0; i < N; i++)
                        v[i] = NextDouble() * StdDev + Mean;

                    return new DoubleVector(v);
            }
        }

		/// <summary>
		/// Gets a normally distributed random value. Distribution has a mean of 0 and standard deviation of 1.
		/// </summary>
		/// <returns>Random value.</returns>
		private static double NextDouble()
        {
			double U1 = Uniform.NextDouble();
			double U2 = Uniform.NextDouble();
			double r = Math.Sqrt(-2.0 * Math.Log(U1));

			r *= Math.Sin(2.0 * Math.PI * U2);

			return r;
		}

	}
}
