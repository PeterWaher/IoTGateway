using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Count(v) iterative evaluator
	/// </summary>
	public class CountEvaluator : IIterativeEvaluator
	{
		private long count = 0;

		/// <summary>
		/// Count(v) iterative evaluator
		/// </summary>
		public CountEvaluator()
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.count = 0;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.count++;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return new DoubleNumber(this.count);
		}
	}
}