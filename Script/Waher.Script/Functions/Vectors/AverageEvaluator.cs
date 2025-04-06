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
		private double doubleSum = 0;
		private bool isDouble = true;
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
			this.doubleSum = 0;
			this.isDouble = true;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.isDouble && Element is DoubleNumber D)
				this.doubleSum += D.Value;
			else if (this.sum is null)
			{
				this.isDouble = false;

				if (this.count == 0)
					this.sum = Element;
				else
					this.sum = Add.EvaluateAddition(new DoubleNumber(this.doubleSum), Element, this.node);
			}
			else
				this.sum = Add.EvaluateAddition(this.sum, Element, this.node);

			this.count++;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			if (this.count == 0)
				return ObjectValue.Null;
			else if (this.isDouble)
				return new DoubleNumber(this.doubleSum / this.count);
			else if (this.count == 1)
				return this.sum;
			else
				return Divide.EvaluateDivision(this.sum, new DoubleNumber(this.count), this.node);
		}
	}
}
