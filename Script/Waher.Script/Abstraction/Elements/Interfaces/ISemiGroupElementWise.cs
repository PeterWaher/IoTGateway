using System;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of element-wise semigroup elements.
	/// </summary>
	public interface ISemiGroupElementWise : ISemiGroupElement
	{
		/// <summary>
		/// Tries to add an element to the current element, from the left, element-wise.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ISemiGroupElementWise AddLeftElementWise(ISemiGroupElementWise Element);

		/// <summary>
		/// Tries to add an element to the current element, from the right, element-wise.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ISemiGroupElementWise AddRightElementWise(ISemiGroupElementWise Element);
	}
}
