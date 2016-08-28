using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Min(v)
	/// </summary>
	public class Min : FunctionOneVectorVariable
	{
		/// <summary>
		/// Min(v)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Min(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "min"; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
		{
			double[] v = Argument.Values;

			if (v.Length == 0)
				return ObjectValue.Null;

			return new DoubleNumber(CalcMin(Argument.Values, this));
		}

		/// <summary>
		/// Returns the smallest value.
		/// </summary>
		/// <param name="Values">Set of values. Must not be empty.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Smallest value.</returns>
		public static double CalcMin(double[] Values, ScriptNode Node)
		{
			double Result;
			int i, c = Values.Length;
			double d;

			if (c == 0)
				throw new ScriptRuntimeException("Empty set of values.", Node);

			Result = Values[0];

			for (i = 1; i < c; i++)
			{
				d = Values[i];
				if (d < Result)
					Result = d;
			}

			return Result;
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
		{
			return CalcMin(Argument, this);
		}

		/// <summary>
		/// Returns the smallest value.
		/// </summary>
		/// <param name="Values">Set of values. Must not be empty.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Smallest value.</returns>
		public static IElement CalcMin(IVector Values, ScriptNode Node)
		{
			IElement Result = null;
			IOrderedSet S = null;

			foreach (IElement E in Values.ChildElements)
			{
				if (Result == null || S.Compare(Result, E) > 0)
				{
					Result = E;
					S = Result.AssociatedSet as IOrderedSet;
					if (S == null)
						throw new ScriptRuntimeException("Cannot compare operands.", Node);
				}
			}

			if (Result == null)
				return ObjectValue.Null;
			else
				return Result;
		}
	}
}