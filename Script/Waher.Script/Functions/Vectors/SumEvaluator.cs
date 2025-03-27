using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
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
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.sum is null)
				this.sum = Element;
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
