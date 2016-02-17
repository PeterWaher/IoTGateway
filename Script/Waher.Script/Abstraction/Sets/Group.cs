using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of groups.
	/// </summary>
	public abstract class Group : SemiGroup, IGroup
	{
		/// <summary>
		/// Base class for all types of groups.
		/// </summary>
		public Group()
			: base()
		{
		}

		/// <summary>
		/// Subtracts the right group element from the left one: Left+(-Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IGroupElement RightSubtract(IGroupElement Left, IGroupElement Right)
		{
			return this.Add(Left, Right.Negate()) as IGroupElement;
		}

		/// <summary>
		/// Subtracts the left group element from the right one: (-Left)+Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IGroupElement LeftSubtract(IGroupElement Left, IGroupElement Right)
		{
			return this.Add(Left.Negate(), Right) as IGroupElement;
		}

		/// <summary>
		/// If the group + operator is commutative or not.
		/// </summary>
		public abstract bool IsAbelian
		{
			get;
		}

		/// <summary>
		/// Returns the additive identity of the group.
		/// </summary>
		public abstract IGroupElement AdditiveIdentity
		{
			get;
		}

	}
}
