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
	/// Generates a random number using the cauchy distribution.
	/// </summary>
	public class Cauchy : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };

		/// <summary>
		/// Generates a random number using the cauchy distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Cauchy(ScriptNode Shape, ScriptNode Scale, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale, N }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the cauchy distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Cauchy(ScriptNode Shape, ScriptNode Scale, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "cauchy"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "median", "scale", "N" }; }
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
					double Median = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					double Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

					return new DoubleNumber(NextDouble(Median, Scale));

				case 3:
				default:
					Median = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
					double d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
					int N = (int)Math.Round(d);
					if (N < 0 || N != d)
						throw new ScriptRuntimeException("N must be a non-negative integer.", this);

					double[] v = new double[N];
					int i;

					for (i = 0; i < N; i++)
						v[i] = NextDouble(Median, Scale);

					return new DoubleVector(v);
			}
		}

		/// <summary>
		/// Gets a cauchy distributed random value.
		/// </summary>
		/// <param name="Median">Median of distribution.</param>
		/// <param name="Scale">Scale of distribution.</param>
		/// <returns>Random value.</returns>
		public static double NextDouble(double Median, double Scale)
		{
			if (Scale <= 0)
				throw new ArgumentOutOfRangeException("Must be positive.", nameof(Scale));

			return Math.Tan(Math.PI * (Uniform.NextDouble() - 0.5)) * Scale + Median;
		}

	}

}
