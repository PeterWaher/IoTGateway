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
	/// Generates a random number using the laplace distribution.
	/// </summary>
	public class Laplace : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };

		/// <summary>
		/// Generates a random number using the laplace distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Laplace(ScriptNode Shape, ScriptNode Scale, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale, N }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the laplace distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Laplace(ScriptNode Shape, ScriptNode Scale, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "laplace"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "mean", "scale", "N" }; }
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
					double Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					double Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

					return new DoubleNumber(NextDouble(Mean, Scale));

				case 3:
				default:
					Mean = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
					double d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
					int N = (int)Math.Round(d);
					if (N < 0 || N != d)
						throw new ScriptRuntimeException("N must be a non-negative integer.", this);

					double[] v = new double[N];
					int i;

					for (i = 0; i < N; i++)
						v[i] = NextDouble(Mean, Scale);

					return new DoubleVector(v);
			}
		}

		/// <summary>
		/// Gets a laplace distributed random value.
		/// </summary>
		/// <param name="Median">Median of distribution.</param>
		/// <param name="Scale">Scale of distribution.</param>
		/// <returns>Random value.</returns>
		public static double NextDouble(double Mean, double Scale)
		{
			double u = Uniform.NextDouble();

			if (u < 0.5)
				return Mean + Scale * Math.Log(2 * u);
			else
				return Mean - Scale * Math.Log(2 * (1 - u));
		}

	}

}
