using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// First(v) iterative evaluator
    /// </summary>
    public class FirstEvaluator : IIterativeEvaluator
    {
		private IElement first = null;

		/// <summary>
		/// First(v) iterative evaluator
		/// </summary>
		public FirstEvaluator()
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.first = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
            if (this.first is null)
				this.first = Element;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
            return this.first ?? ObjectValue.Null;
		}
	}
}
