using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.CustomOperators
{
	/// <summary>
	/// Interface for binary operations.
	/// </summary>
	public interface IBinaryOperation
	{
		/// <summary>
		/// Name of operator.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Left operand.
		/// </summary>
		IElement Left { get; }

		/// <summary>
		/// Right operand.
		/// </summary>
		IElement Right { get; }
	}
}
