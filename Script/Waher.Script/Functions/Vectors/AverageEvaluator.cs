using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Average(v), Avg(v) iterative evaluator
	/// </summary>
	public class AverageEvaluator : IIterativeEvaluator
	{
		private readonly Average node;
		private IElement sum = null;
		private long count = 0;

		/// <summary>
		/// Average(v), Avg(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node reference</param>
		public AverageEvaluator(Average Node)
		{
			this.node = Node;
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
				this.sum = Add.EvaluateAddition(this.sum, Element, this.node);
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
				return Divide.EvaluateDivision(this.sum, new DoubleNumber(this.count), this.node);
		}
	}
}
