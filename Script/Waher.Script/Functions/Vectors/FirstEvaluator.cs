using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// First(v) iterative evaluator
    /// </summary>
    public class FirstEvaluator : UnaryIterativeEvaluator
	{
		private IElement first = null;

		/// <summary>
		/// First(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public FirstEvaluator(First Node)
			: base(Node.Argument)
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.first = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
            if (this.first is null)
				this.first = Element;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
            return this.first ?? ObjectValue.Null;
		}
	}
}
