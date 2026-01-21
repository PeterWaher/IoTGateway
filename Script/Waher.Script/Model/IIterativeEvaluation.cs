namespace Waher.Script.Model
{
	/// <summary>
	/// A function that can be iteratively evaluated, meaning it can be iteratively
	/// computed one element at a time, not requiring access to all elements in a vector
	/// to compute the final result.
	/// </summary>
	public interface IIterativeEvaluation
	{
		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		bool CanEvaluateIteratively { get; }

		/// <summary>
		/// Creates an iterative evaluator for the node.
		/// </summary>
		/// <returns>Iterative evaluator reference.</returns>
		IIterativeEvaluator CreateEvaluator();
	}
}
