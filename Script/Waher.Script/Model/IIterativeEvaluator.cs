using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// A function that is an interative evaluator, meaning it can be iteratively
	/// computed one element at a time, not requiring access to all elements in a vector
	/// to compute the final result.
	/// </summary>
	public interface IIterativeEvaluator : IFunction
	{
		/// <summary>
		/// If the evaluator can perform the computation iteratively.
		/// </summary>
		bool CanEvaluateIteratively { get; }

		/// <summary>
		/// Creates a new instance of the iterative evaluator.
		/// </summary>
		/// <returns>Reference to new instance.</returns>
		IIterativeEvaluator CreateNewEvaluator();

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
