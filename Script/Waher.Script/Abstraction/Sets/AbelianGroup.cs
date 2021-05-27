using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of abelian groups.
	/// </summary>
	public abstract class AbelianGroup : Group, IAbelianGroup
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
		public override sealed IGroupElement RightSubtract(IGroupElement Left, IGroupElement Right)
		{
			if (Left is IAbelianGroupElement L && Right is IAbelianGroupElement R)
				return this.Subtract(L, R);
			else
				return base.RightSubtract(Left, Right);
		}

		/// <summary>
		/// Subtracts the left group element from the right one: (-Left)+Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override sealed IGroupElement LeftSubtract(IGroupElement Left, IGroupElement Right)
		{
			if (Left is IAbelianGroupElement L && Right is IAbelianGroupElement R)
				return this.Subtract(R, L);
			else
				return base.LeftSubtract(Left, Right);
		}

		/// <summary>
		/// Subtracts the right group element from the left one: Left-Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IAbelianGroupElement Subtract(IAbelianGroupElement Left, IAbelianGroupElement Right)
		{
			return this.Add(Left, Right.Negate()) as IAbelianGroupElement;
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
		public override sealed IGroupElement AdditiveIdentity
		{
			get
			{
				return this.Zero;
			}
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public abstract IAbelianGroupElement Zero
		{
			get;
		}

	}
}
