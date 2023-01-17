using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Custom external property evaluator.
	/// </summary>
	public interface IPropertyEvaluator
	{
		/// <summary>
		/// Prepares the evaluator with its definition.
		/// </summary>
		/// <param name="Definition">Property definition</param>
		Task Prepare(string Definition);

		/// <summary>
		/// Evaluates the property evaluator, on an object instance.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value.</returns>
		Task<object> Evaluate(object Instance);
	}
}
