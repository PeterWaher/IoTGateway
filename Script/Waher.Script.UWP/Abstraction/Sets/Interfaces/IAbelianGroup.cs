using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of abelian groups.
	/// </summary>
	public interface IAbelianGroup : IGroup
	{
		/// <summary>
		/// Subtracts the right group element from the left one: Left-Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IAbelianGroupElement Subtract(IAbelianGroupElement Left, IAbelianGroupElement Right);

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		IAbelianGroupElement Zero
		{
			get;
		}

	}
}
