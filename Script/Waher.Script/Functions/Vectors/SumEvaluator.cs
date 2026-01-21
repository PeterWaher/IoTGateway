using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Sum(v) iterative evaluator
	/// </summary>
	public class SumEvaluator : UnaryIterativeEvaluator
	{
		private IElement sum = null;
		private double doubleSum = 0;
		private bool isDouble = true;

		/// <summary>
		/// Sum(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public SumEvaluator(Sum Node)
			: base(Node.Argument)
        {
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.sum = null;
			this.doubleSum = 0;
			this.isDouble = true;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			if (this.isDouble && Element is DoubleNumber D)
				this.doubleSum += D.Value;
			else if (this.sum is null)
			{
				this.isDouble = false;

				if (this.sum is null)
					this.sum = Element;
				else
					this.sum = Add.EvaluateAddition(new DoubleNumber(this.doubleSum), Element, this.Node);
			}
			else
				this.sum = Add.EvaluateAddition(this.sum, Element, this.Node);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			if (this.isDouble)
				return new DoubleNumber(this.doubleSum);
			else 
				return this.sum ?? ObjectValue.Null;
		}
	}
}
