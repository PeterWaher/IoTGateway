using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of abelian group elements.
	/// </summary>
	public interface IAbelianGroupElement : IGroupElement
	{
		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IAbelianGroupElement Add(IAbelianGroupElement Element);

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		IAbelianGroup AssociatedAbelianGroup
		{
			get;
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		IAbelianGroupElement Zero
		{
			get;
		}
	}
}
