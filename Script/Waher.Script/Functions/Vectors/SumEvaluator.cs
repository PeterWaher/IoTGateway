using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Sum(v) iterative evaluator
	/// </summary>
	public class SumEvaluator : IIterativeEvaluator
    {
        private readonly Sum node;
		private IElement sum = null;
		private double doubleSum = 0;
		private bool isDouble = true;

		/// <summary>
		/// Sum(v) iterative evaluator
		/// </summary>
		public SumEvaluator(Sum Node)
        {
            this.node = Node;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.sum = null;
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

				if (this.sum is null)
					this.sum = Element;
				else
					this.sum = Add.EvaluateAddition(new DoubleNumber(this.doubleSum), Element, this.node);
			}
			else
				this.sum = Add.EvaluateAddition(this.sum, Element, this.node);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			if (this.sum is null)
				return ObjectValue.Null;
			else 
				return this.sum;
		}
	}
}
