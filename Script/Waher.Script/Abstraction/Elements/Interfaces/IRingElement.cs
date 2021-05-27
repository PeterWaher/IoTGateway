using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of ring elements.
	/// </summary>
	public interface IRingElement : IAbelianGroupElement
	{
		/// <summary>
		/// Tries to multiply an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRingElement MultiplyLeft(IRingElement Element);

		/// <summary>
		/// Tries to multiply an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRingElement MultiplyRight(IRingElement Element);

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		IRingElement Invert();

		/// <summary>
		/// Associated Ring.
		/// </summary>
		IRing AssociatedRing
		{
			get;
		}
	}
}
