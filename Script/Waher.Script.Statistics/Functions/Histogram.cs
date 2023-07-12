using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Constants;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Statistics.Functions
{
	/// <summary>
	/// Computes a histogram from a set of data.
	/// </summary>
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
		public override string FunctionName => nameof(Histogram);

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

			double[] Result;
			double Diff = Max - Min;

			if (Data is DoubleVector v)
				Result = Compute(v.Values, N, Min, Max);
			else
				Result = Compute(Data, N, Min, Max);

			return new ObjectVector(new IElement[]
			{
				new ObjectVector(GetLabels(N, Min, Diff)),
				new DoubleVector(Result)
			});
		}

		/// <summary>
		/// Computes a Histogram over an array of floating-point values.
		/// </summary>
		/// <param name="Data">Values to process.</param>
		/// <param name="N">Number of buckets.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="Max">Maximum value.</param>
		/// <returns>Histogram.</returns>
		public static double[] Compute(double[] Data, int N, double Min, double Max)
		{
			double[] Result = new double[N];
			double Diff = Max - Min;
			double x;
			int i, j;
			int c = Data.Length;

			for (j = 0; j < c; j++)
			{
				x = Data[j];

				if (x >= Min && x <= Max)
				{
					i = (int)(((x - Min) * N) / Diff);
					if (i == N)
						i--;

					Result[i]++;
				}
			}

			return Result;
		}

		/// <summary>
		/// Computes a Histogram over an array of floating-point values.
		/// </summary>
		/// <param name="Data">Values to process.</param>
		/// <param name="N">Number of buckets.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="Max">Maximum value.</param>
		/// <returns>Histogram.</returns>
		public static double[] Compute(IVector Data, int N, double Min, double Max)
		{
			double[] Result = new double[N];
			double Diff = Max - Min;
			double x;
			int i;

			foreach (IElement E in Data.VectorElements)
			{
				if (E.AssociatedObjectValue is double x2)
					x = x2;
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
					i = (int)(((x - Min) * N) / Diff);
					if (i == N)
						i--;

					Result[i]++;
				}
			}

			return Result;
		}

		/// <summary>
		/// Computes a Histogram over an array of floating-point values.
		/// </summary>
		/// <param name="Data">Values to process.</param>
		/// <param name="N">Number of buckets.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="Max">Maximum value.</param>
		/// <returns>Histogram.</returns>
		public static double[] Compute(Array Data, int N, double Min, double Max)
		{
			double[] Result = new double[N];
			double Diff = Max - Min;
			double x;
			int i;

			foreach (object Element in Data)
			{
				if (Element is double x2)
					x = x2;
				else
				{
					try
					{
						x = Expression.ToDouble(Element);
					}
					catch (Exception)
					{
						continue;
					}
				}

				if (x >= Min && x <= Max)
				{
					i = (int)(((x - Min) * N) / Diff);
					if (i == N)
						i--;

					Result[i]++;
				}
			}

			return Result;
		}

		internal static string[] GetLabels(int N, double Min, double Diff)
		{
			string[] Labels = new string[N];
			string CurrentLabel;
			double Next = Min;
			string NextLabel = TrimLabel(Expression.ToString(Next));
			int i;

			for (i = 0; i < N; i++)
			{
				CurrentLabel = NextLabel;
				Next = ((i + 1) * Diff) / N + Min;
				NextLabel = TrimLabel(Expression.ToString(Next));
				Labels[i] = CurrentLabel + "-" + NextLabel;
			}

			return Labels;
		}

		/// <summary>
		/// Trims a numeric label, removing apparent roundoff errors.
		/// </summary>
		/// <param name="Label">Numeric label.</param>
		/// <returns>Trimmed label.</returns>
		public static string TrimLabel(string Label)
		{
			int i = Label.IndexOf('.');
			if (i < 0)
				return Label;

			int j = Label.LastIndexOf("00000");
			if (j > i)
			{
				i = Label.Length - 5;
				while (Label[i] == '0')
					i--;

				if (Label[i] == '.')
					i--;

				return Label.Substring(0, i + 1);
			}

			j = Label.LastIndexOf("99999");
			if (j > i)
			{
				bool DecimalSection = true;

				i = j;
				while (Label[i] == '9')
					i--;

				if (Label[i] == '.')
				{
					i--;
					DecimalSection = false;
				}

				char[] ch = Label.Substring(0, i + 1).ToCharArray();
				char ch2;
				bool CheckOne = true;

				while (i >= 0)
				{
					ch2 = ch[i];
					if (ch2 == '9')
					{
						ch[i] = '0';
						i--;
					}
					else if (ch2 == '.')
					{
						i--;
						DecimalSection = false;
					}
					else if (ch2 >= '0' && ch2 <= '8')
					{
						ch[i]++;
						CheckOne = false;
						break;
					}
					else
						break;
				}

				if (CheckOne)
				{
					int c = ch.Length;
					i++;
					Array.Resize(ref ch, c + 1);
					Array.Copy(ch, i, ch, i + 1, c - i);
					ch[i] = '1';
				}
				else if (DecimalSection)
				{
					i = ch.Length - 1;
					while (i >= 0 && ch[i] == '0')
						i--;

					if (ch[i] == '.')
						i--;

					if (i < ch.Length - 1)
						Array.Resize(ref ch, i + 1);
				}

				Label = new string(ch);
			}

			return Label;
		}

	}
}
