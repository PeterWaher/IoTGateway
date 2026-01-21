using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Product(v) iterative evaluator
	/// </summary>
	public class ProductEvaluator : UnaryIterativeEvaluator
	{
		private IElement product = null;
		private double doubleProduct = 0;
		private bool isDouble = true;

		/// <summary>
		/// Product(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public ProductEvaluator(Product Node)
			: base(Node.Argument)
        {
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.product = null;
			this.doubleProduct = 0;
			this.isDouble = true;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			if (this.isDouble && Element is DoubleNumber D)
				this.doubleProduct *= D.Value;
			else if (this.product is null)
			{
				this.isDouble = false;

				if (this.product is null)
					this.product = Element;
				else
					this.product = Multiply.EvaluateMultiplication(new DoubleNumber(this.doubleProduct), Element, this.Node);
			}
			else
				this.product = Multiply.EvaluateMultiplication(this.product, Element, this.Node);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			if (this.isDouble)
				return new DoubleNumber(this.doubleProduct);
			else 
				return this.product ?? ObjectValue.Null;
		}
	}
}
