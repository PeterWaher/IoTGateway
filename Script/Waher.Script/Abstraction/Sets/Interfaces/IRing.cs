using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of rings.
	/// </summary>
	public interface IRing : IAbelianGroup
	{
		/// <summary>
		/// Multiplies two ring elements, if possible.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRingElement Multiply(IRingElement Left, IRingElement Right);

		/// <summary>
		/// Divides the right ring element from the left one: Left*(1/Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRingElement RightDivide(IRingElement Left, IRingElement Right);

		/// <summary>
		/// Divides the left ring element from the right one: (1/Left)*Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRingElement LeftDivide(IRingElement Left, IRingElement Right);

		/// <summary>
		/// If the ring * operator is commutative or not.
		/// </summary>
		bool IsCommutative
		{
			get;
		}
	}
}
