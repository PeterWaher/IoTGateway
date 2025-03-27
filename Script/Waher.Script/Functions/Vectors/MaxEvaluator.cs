using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Max(v) iterative evaluator
	/// </summary>
	public class MaxEvaluator : IIterativeEvaluator
    {
		private readonly Max node;
		private IElement max = null;
		private IOrderedSet maxSet = null;

		/// <summary>
		/// Max(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node reference</param>
		public MaxEvaluator(Max Node)
        {
			this.node = Node;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.max = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
            if (this.max is null || this.maxSet.Compare(this.max, Element) < 0)
            {
				if (!(Element.AssociatedSet is IOrderedSet S))
					throw new ScriptRuntimeException("Cannot compare operands.", this.node);

				this.max = Element;
                this.maxSet = S;
            }
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
            return this.max ?? ObjectValue.Null;
		}
	}
}
