using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Abstraction.CustomOperators
{
    /// <summary>
    /// Interface for custom binary operators.
    /// </summary>
    public interface IBinaryOperator : IProcessingSupport<IBinaryOperation>
	{
        /// <summary>
        /// Evaluates the custom binary operator.
        /// </summary>
        /// <param name="Operation">Binary operation to evaluate.</param>
        /// <param name="Node">Node performing the evaluation.</param>
        /// <returns>Result of the evaluation.</returns>
        IElement Evaluate(IBinaryOperation Operation, ScriptNode Node);
	}
}
