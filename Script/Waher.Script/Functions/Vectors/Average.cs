using System.Numerics;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Average(v), Avg(v)
	/// </summary>
	public class Average : FunctionOneVectorVariable, IIterativeEvaluator
	{
		/// <summary>
		/// Average(v), Avg(v)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Average(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Average);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "avg" };

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
		{
			return EvaluateAverage(Argument, this);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
		{
			return new DoubleNumber(CalcAverage(Argument.Values, this));
		}

		/// <summary>
		/// Calculates the average of a set of double values.
		/// </summary>
		/// <param name="Values">Values</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Average.</returns>
		public static double CalcAverage(double[] Values, ScriptNode Node)
		{
			double Result = 0;
			int i, c = Values.Length;

			if (c == 0)
				throw new ScriptRuntimeException("Empty vector.", Node);

			for (i = 0; i < c; i++)
				Result += Values[i];

			Result /= c;

			return Result;
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
		{
			return new ComplexNumber(CalcAverage(Argument.Values, this));
		}

		/// <summary>
		/// Calculates the average of a set of double values.
		/// </summary>
		/// <param name="Values">Values</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Average.</returns>
		public static Complex CalcAverage(Complex[] Values, ScriptNode Node)
		{
			Complex Result = Complex.Zero;
			int i, c = Values.Length;

			if (c == 0)
				throw new ScriptRuntimeException("Empty vector.", Node);

			for (i = 0; i < c; i++)
				Result += Values[i];

			Result /= c;

			return Result;
		}

		/// <summary>
		/// Calculates the average of the elements of a vector.
		/// </summary>
		/// <param name="Vector">Vector</param>
		/// <param name="Node">Node performing evaluation.</param>
		/// <returns>Average of elements.</returns>
		public static IElement EvaluateAverage(IVector Vector, ScriptNode Node)
		{
			IElement Result = Sum.EvaluateSum(Vector, Node);
			int n = Vector.Dimension;

			if (Result is null)
				return ObjectValue.Null;
			else
			{
				IRingElement Avg;

				if (Result is IRingElement RE && !((Avg = RE.MultiplyRight(new DoubleNumber(1.0 / n))) is null))
					return Avg;
				else
					return Operators.Arithmetics.Divide.EvaluateDivision(Result, new DoubleNumber(n), Node);
			}
		}

		#region IIterativeEvalautor

		private IElement sum = null;
		private long count = 0;

		/// <summary>
		/// If the evaluator can perform the computation iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

		/// <summary>
		/// Creates a new instance of the iterative evaluator.
		/// </summary>
		/// <returns>Reference to new instance.</returns>
		public IIterativeEvaluator CreateNewEvaluator()
		{
			return new Average(this.Argument, this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.sum = null;
			this.count = 0;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.sum is null)
			{
				this.sum = Element;
				this.count = 1;
			}
			else
			{
				this.sum = Add.EvaluateAddition(this.sum, Element, this);
				this.count++;
			}
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			if (this.sum is null)
				return ObjectValue.Null;
			else if (this.count == 1)
				return this.sum;
			else
				return Divide.EvaluateDivision(this.sum, new DoubleNumber(this.count), this);
		}

		#endregion

	}
}
