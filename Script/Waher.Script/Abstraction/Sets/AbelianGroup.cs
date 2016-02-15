using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of abelian groups.
	/// </summary>
	public abstract class AbelianGroup : Group
	{
		/// <summary>
		/// Base class for all types of abelian groups.
		/// </summary>
		public AbelianGroup()
			: base()
		{
		}

		/// <summary>
		/// Subtracts the right group element from the left one: Left+(-Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override sealed GroupElement RightSubtract(GroupElement Left, GroupElement Right)
		{
			AbelianGroupElement L = Left as AbelianGroupElement;
			AbelianGroupElement R = Right as AbelianGroupElement;

			if (L == null || R == null)
				return base.RightSubtract(Left, Right);
			else
				return this.Subtract(L, R);
		}

		/// <summary>
		/// Subtracts the left group element from the right one: (-Left)+Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override sealed GroupElement LeftSubtract(GroupElement Left, GroupElement Right)
		{
			AbelianGroupElement L = Left as AbelianGroupElement;
			AbelianGroupElement R = Right as AbelianGroupElement;

			if (L == null || R == null)
				return base.LeftSubtract(Left, Right);
			else
				return this.Subtract(R, L);
		}

		/// <summary>
		/// Subtracts the right group element from the left one: Left-Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual GroupElement Subtract(AbelianGroupElement Left, AbelianGroupElement Right)
		{
			return this.Add(Left, Right.Negate()) as GroupElement;
		}

		/// <summary>
		/// If the group + operator is commutative or not. (For abelian groups, this is always true.)
		/// </summary>
		public override sealed bool IsAbelian
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns the additive identity of the group. (For abelian groups, this calls <see cref="Zero"/>.)
		/// </summary>
		public override sealed GroupElement AdditiveIdentity
		{
			get
			{
				return this.Zero;
			}
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public abstract AbelianGroupElement Zero
		{
			get;
		}

	}
}
