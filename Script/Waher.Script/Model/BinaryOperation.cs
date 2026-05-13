using Waher.Script.Abstraction.CustomOperators;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Information about a binary operation for use with custom binary operators.
	/// </summary>
	public class BinaryOperation : IBinaryOperation
	{
		/// <summary>
		/// Information about a binary operation for use with custom binary operators.
		/// </summary>
		/// <param name="Name">Name of the operator.</param>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		public BinaryOperation(string Name, IElement Left, IElement Right)
		{
			this.Name = Name;
			this.Left = Left;
			this.Right = Right;
		}

		/// <summary>
		/// Name of operator.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Left operand.
		/// </summary>
		public IElement Left { get; }

		/// <summary>
		/// Right operand.
		/// </summary>
		public IElement Right { get; }
	}
}
