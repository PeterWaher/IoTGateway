using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// An interative evaluator of a function supporting the <see cref="IIterativeEvaluation"/>
	/// interface.
	/// </summary>
	public interface IIterativeEvaluator
	{
		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		void RestartEvaluator();

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		void AggregateElement(IElement Element);

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		IElement GetAggregatedResult();
	}
}
