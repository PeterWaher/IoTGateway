using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// An interative evaluator of a function or operation defined by a single 
	/// <see cref="ScriptNode"/>.
	/// </summary>
	public abstract class UnaryIterativeEvaluator : IIterativeEvaluator
	{
		private readonly ScriptNode node;

		/// <summary>
		/// An interative evaluator of a function or operation defined by a single 
		/// <see cref="ScriptNode"/>.
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public UnaryIterativeEvaluator(ScriptNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Node being iteratively evaluated.
		/// </summary>
		public ScriptNode Node => this.node;

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public abstract void RestartEvaluator();

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public abstract IElement GetAggregatedResult();

		/// <summary>
		/// If the evaluator uses the current element in its aggregate evaluation.
		/// </summary>
		public virtual bool UsesElement => true;

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public abstract void AggregateElement(IElement Element);

		/// <summary>
		/// If the evaluator include asynchronous evaluation. Asynchronous evaluators
		/// should be evaluated using <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public bool IsAsynchronous => this.node.IsAsynchronous;

		/// <summary>
		/// Evaluates the evaluator, using the variables provided in the <paramref name="Variables"/> 
		/// collection and an object instance being the current object value of the iteration.
		/// This method should be used for nodes whose <see cref="IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(Variables Variables)
		{
			return this.node.Evaluate(Variables);
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
			return this.node.EvaluateAsync(Variables);
		}
	}
}
