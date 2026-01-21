using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Model
{
	/// <summary>
	/// Default iterator for one-vector-variable functions.
	/// </summary>
	public class FunctionOneVectorVariableIterator : IIterativeEvaluator
	{
		private readonly FunctionOneVectorVariable function;
		private readonly ChunkedList<IElement> elements = new ChunkedList<IElement>();

		/// <summary>
		/// Default iterator for one-vector-variable functions.
		/// </summary>
		/// <param name="Function">Function being iterated.</param>
		public FunctionOneVectorVariableIterator(FunctionOneVectorVariable Function)
		{
			this.function = Function;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.elements.Clear();
		}

		/// <summary>
		/// If the evaluator uses the current element in its aggregate evaluation.
		/// </summary>
		public bool UsesElement => true;

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.elements.Add(Element);
		}

		/// <summary>
		/// If the evaluator include asynchronous evaluation. Asynchronous evaluators
		/// should be evaluated using <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public bool IsAsynchronous => this.function.IsAsynchronous;

		/// <summary>
		/// Evaluates the evaluator, using the variables provided in the <paramref name="Variables"/> 
		/// collection and an object instance being the current object value of the iteration.
		/// This method should be used for nodes whose <see cref="IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(Variables Variables)
		{
			return this.function.Evaluate(Variables);
		}

		/// <summary>
		/// Evaluates the evaluator, using the variables provided in the <paramref name="Variables"/> 
		/// collection and an object instance being the current object value of the iteration.
		/// This method should be used for nodes whose <see cref="IsAsynchronous"/> is true.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public Task<IElement> EvaluateAsync(Variables Variables)
		{
			return this.function.EvaluateAsync(Variables);
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			IElement Vector = VectorDefinition.Encapsulate(this.elements.ToArray(), false, this.function);
			return this.function.Evaluate(Vector, null);
		}
	}
}
