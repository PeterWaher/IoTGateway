using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Count(v) iterative evaluator
	/// </summary>
	public class CountEvaluator : UnaryIterativeEvaluator
	{
		private long count = 0;

		/// <summary>
		/// Count(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public CountEvaluator(Count Node)
			: base(Node.Arguments[0])
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.count = 0;
		}

		/// <summary>
		/// If the evaluator uses the current element in its aggregate evaluation.
		/// </summary>
		public override bool UsesElement => false;

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			this.count++;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			return new DoubleNumber(this.count);
		}
	}
}