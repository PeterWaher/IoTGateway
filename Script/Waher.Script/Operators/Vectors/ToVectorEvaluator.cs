using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// ToVector(v) iterative evaluator
	/// </summary>
	public class ToVectorEvaluator : IIterativeEvaluator
	{
		private readonly ChunkedList<IElement> elements = new ChunkedList<IElement>();
		private readonly ToVector node;

		/// <summary>
		/// ToVector(v) iterative evaluator
		/// </summary>
		/// <param name="Node">Node reference</param>
		public ToVectorEvaluator(ToVector Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.elements.Clear();
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.elements.Add(Element);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return VectorDefinition.Encapsulate(this.elements.ToArray(), false, this.node);
		}
	}
}
