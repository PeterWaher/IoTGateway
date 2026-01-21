using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Wildcard (*) iterative evaluator
	/// </summary>
	public class WildcardEvaluator : LeafIterativeEvaluator
	{
		private readonly ChunkedList<IElement> elements = new ChunkedList<IElement>();

		/// <summary>
		/// Wildcard (*) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public WildcardEvaluator(Wildcard Node)
			: base(Node)
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
			return VectorDefinition.Encapsulate(this.elements, false, this.Node);
		}

		/// <summary>
		/// Evaluates the evaluator, using the variables provided in the <paramref name="Variables"/> 
		/// collection and an object instance being the current object value of the iteration.
		/// This method should be used for nodes whose <see cref="LeafIterativeEvaluator.IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (Variables is ObjectProperties Properties)
				return Expression.Encapsulate(Properties.Object);
			else
				return ObjectValue.Null;
		}
	}
}
