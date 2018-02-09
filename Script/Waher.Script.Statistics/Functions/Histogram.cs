using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Statistics.Functions
{
	public class Histogram : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes4Parameters = new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Computes a histogram from a set of data.
		/// </summary>
		/// <param name="Data">Data</param>
		/// <param name="Min">Smallest value</param>
		/// <param name="Max">Largest value</param>
		/// <param name="N">Number of buckets</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Histogram(ScriptNode Data, ScriptNode Min, ScriptNode Max, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Data, Min, Max, N }, argumentTypes4Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "histogram"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "data", "min", "max", "N" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IVector Data = Arguments[0] as IVector ?? throw new ScriptRuntimeException("First argument must be a vector.", this);
			double Min = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double Max = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			double d = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
			int N = (int)Math.Round(d);
			if (N <= 0 || N != d)
				throw new ScriptRuntimeException("N must be a positive integer.", this);

			if (Max <= Min)
				throw new ScriptRuntimeException("Min must be smaller than Max.", this);

			double[] Result = new double[N];
			double Scale = N / (Max - Min);
			double x;
			int i, j, c;

			if (Data is DoubleVector v)
			{
				double[] w = v.Values;
				c = w.Length;

				for (j = 0; j < c; j++)
				{
					x = w[j];

					if (x >= Min && x <= Max)
					{
						i = (int)(x * Scale);
						if (i == N)
							i--;

						Result[i]++;
					}
				}
			}
			else
			{
				foreach (IElement E in Data.VectorElements)
				{
					if (E is DoubleNumber X)
						x = X.Value;
					else
					{
						try
						{
							x = Expression.ToDouble(E.AssociatedObjectValue);
						}
						catch (Exception)
						{
							continue;
						}
					}

					if (x >= Min && x <= Max)
					{
						i = (int)(x * Scale);
						if (i == N)
							i--;

						Result[i]++;
					}
				}
			}

			string[] Labels = new string[N];

			Scale = (Max - Min) / N;

			for (i = 0; i < N; i++)
				Labels[i] = Expression.ToString(Min + i * Scale) + "-" + Expression.ToString(Min + (i + 1) * Scale);

			return new ObjectVector(new IElement[]
			{
				new ObjectVector(Labels),
				new DoubleVector(Result)
			});
		}

	}
}
