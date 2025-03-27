using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Min(v) iterative evaluator
	/// </summary>
	public class MinEvaluator : IIterativeEvaluator
	{
		private readonly Min node;
		private IElement min = null;
		private IOrderedSet minSet = null;

		/// <summary>
		/// Min(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node reference</param>
		public MinEvaluator(Min Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.min = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.min is null || this.minSet.Compare(this.min, Element) > 0)
			{
				if (!(Element.AssociatedSet is IOrderedSet S))
					throw new ScriptRuntimeException("Cannot compare operands.", this.node);

				this.min = Element;
				this.minSet = S;
			}
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return this.min ?? ObjectValue.Null;
		}
	}
}