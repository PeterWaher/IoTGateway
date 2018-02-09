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
	/// Generates a random number using the chi squared distribution.
	/// </summary>
	public class Chi2 : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes1Parameters = new ArgumentType[] { ArgumentType.Scalar, };

		/// <summary>
		/// Generates a random number using the chi squared distribution.
		/// </summary>
		/// <param name="Degrees">Degrees of freedom.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Chi2(ScriptNode Degrees, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Degrees, N }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the chi squared distribution.
		/// </summary>
		/// <param name="Degrees">Degrees of freedom.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Chi2(ScriptNode Degrees, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Degrees }, argumentTypes1Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "chi2"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "degrees", "N" }; }
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
				case 1:
					double Degrees = Expression.ToDouble(Arguments[0].AssociatedObjectValue);

					return new DoubleNumber(NextDouble(Degrees));

				case 2:
				default:
					Degrees = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					double d = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
					int N = (int)Math.Round(d);
					if (N < 0 || N != d)
						throw new ScriptRuntimeException("N must be a non-negative integer.", this);

					double[] v = new double[N];
					int i;

					for (i = 0; i < N; i++)
						v[i] = NextDouble(Degrees);

					return new DoubleVector(v);
			}
		}

		/// <summary>
		/// Gets a chi squared distributed random value.
		/// </summary>
		/// <param name="Degrees">Degrees of freedom.</param>
		/// <returns>Random value.</returns>
		public static double NextDouble(double Degrees)
		{
			return Gamma.NextDouble(Degrees / 2, 2);
		}

	}

}
