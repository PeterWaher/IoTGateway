using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Last(v) iterative evaluator
    /// </summary>
    public class LastEvaluator : IIterativeEvaluator
    {
		private IElement last = null;

		/// <summary>
		/// Last(v) iterative evaluator
		/// </summary>
		public LastEvaluator()
        {
        }

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.last = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.last = Element;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return this.last ?? ObjectValue.Null;
		}
	}
}
