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
	/// Generates a random number using the gamma distribution.
	/// </summary>
	public class Gamma : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes3Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		private static readonly ArgumentType[] argumentTypes2Parameters = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, };

		/// <summary>
		/// Generates a random number using the gamma distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="N">Number of values to generate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Gamma(ScriptNode Shape, ScriptNode Scale, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale, N }, argumentTypes3Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a random number using the gamma distribution.
		/// </summary>
		/// <param name="Shape">Shape of the distribution.</param>
		/// <param name="Scale">Scale of the distribution.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Gamma(ScriptNode Shape, ScriptNode Scale, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Shape, Scale }, argumentTypes2Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "gamma"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "shape", "scale", "N" }; }
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
					double Shape = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					double Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

					return new DoubleNumber(NextDouble(Shape, Scale));

				case 3:
				default:
					Shape = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
					Scale = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
					double d = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
					int N = (int)Math.Round(d);
					if (N < 0 || N != d)
						throw new ScriptRuntimeException("N must be a non-negative integer.", this);

					double[] v = new double[N];
					int i;

					for (i = 0; i < N; i++)
						v[i] = NextDouble(Scale, Shape);

					return new DoubleVector(v);
			}
		}

		/// <summary>
		/// Gets a gamma distributed random value.
		/// </summary>
		/// <param name="Shape">Shape of distribution.</param>
		/// <param name="Scale">Scale of distribution.</param>
		/// <returns>Random value.</returns>
		public static double NextDouble(double Shape, double Scale)
		{
			double d, c, x, x2, v, u;

			if (Shape >= 1)
			{
				d = Shape - 1.0 / 3;
				c = 1 / Math.Sqrt(9 * d);

				while (true)
				{
					do
					{
						x = Normal.NextDouble();
						v = 1 + c * x;
					}
					while (v <= 0);

					v = v * v * v;
					u = Uniform.NextDouble();
					x2 = x * x;

					if (u < 1 - .0331 * x2 * x2 || Math.Log(u) < 0.5 * x2 + d * (1 - v + Math.Log(v)))
						return Scale * d * v;
				}
			}
			else if (Shape <= 0)
				throw new ArgumentOutOfRangeException("Shape must be positive.", nameof(Shape));
			else
			{
				double g = NextDouble(Shape + 1, 1);
				double w = Uniform.NextDouble();
				return Scale * g * Math.Pow(w, 1 / Shape);
			}
		}

	}

}
