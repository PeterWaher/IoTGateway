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
	/// Generates a random number using the beta distribution.
	/// </summary>
	public class Beta : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };

		/// <summary>
		/// Generates a random number using the beta distribution.
		/// </summary>
		/// <param name="Alpha">Alpha</param>
		/// <param name="Beta">Beta</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Beta(ScriptNode Alpha, ScriptNode Beta, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Alpha, Beta, N }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the beta distribution.
		/// </summary>
		/// <param name="Alpha">Alpha</param>
		/// <param name="Beta">Beta</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Beta(ScriptNode Alpha, ScriptNode Beta, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Alpha, Beta }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "beta"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "alpha", "beta", "N" }; }
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
				case 2:
					double Alpha = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					double Beta = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

					return new DoubleNumber(NextDouble(Alpha, Beta));

				case 3:
				default:
					Alpha = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					Beta = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
					double d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
					int N = (int)Math.Round(d);
					if (N < 0 || N != d)
						throw new ScriptRuntimeException("N must be a non-negative integer.", this);

					double[] v = new double[N];
					int i;

					for (i = 0; i < N; i++)
						v[i] = NextDouble(Alpha, Beta);

					return new DoubleVector(v);
			}
		}

		/// <summary>
		/// Gets a beta distributed random value.
		/// </summary>
		/// <param name="Alpha">Alpha</param>
		/// <param name="Beta">Beta</param>
		/// <returns>Random value.</returns>
		public static double NextDouble(double Alpha, double Beta)
		{
			if (Alpha <= 0)
				throw new ArgumentOutOfRangeException("Must be positive.", nameof(Alpha));

			if (Beta <= 0)
				throw new ArgumentOutOfRangeException("Must be positive.", nameof(Beta));

			double A = Gamma.NextDouble(Alpha, 1);
			double B = Gamma.NextDouble(Beta, 1);

			return A / (A + B);
		}

	}

}
