using Waher.Runtime.Inventory;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions
{
	/// <summary>
	/// Interface for extension functions
	/// </summary>
	public interface IExtensionFunction : IProcessingSupport<string>
	{
		/// <summary>
		/// Minimum number of arguments
		/// </summary>
		int MinArguments { get; }

		/// <summary>
		/// Maximum number of arguments
		/// </summary>
		int MaxArguments { get; }

		/// <summary>
		/// Creates a function node.
		/// </summary>
		/// <param name="Arguments">Parsed arguments.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		/// <returns>Function script node.</returns>
		ScriptNode CreateFunction(ScriptNode[] Arguments, int Start, int Length, Expression Expression);
	}
}
