using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of commutative ring elements.
	/// </summary>
	public interface ICommutativeRingElement : IRingElement 
	{
		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ICommutativeRingElement Multiply(ICommutativeRingElement Element);

		/// <summary>
		/// Associated Commutative Ring.
		/// </summary>
		ICommutativeRing AssociatedCommutativeRing
		{
			get;
		}
	}
}
