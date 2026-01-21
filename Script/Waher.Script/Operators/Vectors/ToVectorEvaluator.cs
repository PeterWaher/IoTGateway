using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// ToVector(v) iterative evaluator
	/// </summary>
	public class ToVectorEvaluator : UnaryIterativeEvaluator
	{
		private readonly ChunkedList<IElement> elements = new ChunkedList<IElement>();

		/// <summary>
		/// ToVector(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public ToVectorEvaluator(ToVector Node)
			: base(Node.Operand)
		{
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.elements.Clear();
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			this.elements.Add(Element);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			return VectorDefinition.Encapsulate(this.elements.ToArray(), false, this.Node);
		}
	}
}
