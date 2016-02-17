using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of semigroup elements.
	/// </summary>
	public interface ISemiGroupElement : IElement
	{
		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ISemiGroupElement AddLeft(ISemiGroupElement Element);

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ISemiGroupElement AddRight(ISemiGroupElement Element);

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		ISemiGroup AssociatedSemiGroup
		{
			get;
		}
	}
}
