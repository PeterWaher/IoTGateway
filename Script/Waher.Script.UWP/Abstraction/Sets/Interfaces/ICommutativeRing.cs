using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of commutative rings.
	/// </summary>
	public interface ICommutativeRing : IRing
	{
		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ICommutativeRingElement Divide(ICommutativeRingElement Left, ICommutativeRingElement Right);
	}
}
