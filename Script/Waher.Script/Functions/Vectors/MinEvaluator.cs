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
		private double? doubleMin = null;
		private bool isDouble = true;

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
			this.doubleMin = null;
			this.isDouble = true;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.isDouble && Element is DoubleNumber D)
			{
				double d = D.Value;

				if (d < this.doubleMin)
					this.doubleMin = d;
			}
			else if (this.min is null || this.minSet.Compare(this.min, Element) > 0)
			{
				if (!(Element.AssociatedSet is IOrderedSet S))
					throw new ScriptRuntimeException("Cannot compare operands.", this.node);

				this.min = Element;
				this.minSet = S;
				this.isDouble = false;
			}
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			if (this.isDouble)
			{
				if (this.doubleMin.HasValue)
					return new DoubleNumber(this.doubleMin.Value);
				else
					return ObjectValue.Null;
			}
			else
				return this.min ?? ObjectValue.Null;
		}
	}
}
