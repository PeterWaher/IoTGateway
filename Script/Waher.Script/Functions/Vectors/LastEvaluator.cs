using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Last(v) iterative evaluator
    /// </summary>
    public class LastEvaluator : UnaryIterativeEvaluator
	{
		private IElement last = null;

		/// <summary>
		/// Last(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public LastEvaluator(Last Node)
			: base(Node.Argument)
        {
        }

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override	void RestartEvaluator()
		{
			this.last = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			this.last = Element;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			return this.last ?? ObjectValue.Null;
		}
	}
}
