using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of groups.
	/// </summary>
	public interface IGroup : ISemiGroup
	{
		/// <summary>
		/// Subtracts the right group element from the left one: Left+(-Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IGroupElement RightSubtract(IGroupElement Left, IGroupElement Right);

		/// <summary>
		/// Subtracts the left group element from the right one: (-Left)+Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IGroupElement LeftSubtract(IGroupElement Left, IGroupElement Right);

		/// <summary>
		/// If the group + operator is commutative or not.
		/// </summary>
		bool IsAbelian
		{
			get;
		}

		/// <summary>
		/// Returns the additive identity of the group.
		/// </summary>
		IGroupElement AdditiveIdentity
		{
			get;
		}

	}
}
