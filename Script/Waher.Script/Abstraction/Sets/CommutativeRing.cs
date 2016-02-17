using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of commutative rings.
	/// </summary>
	public abstract class CommutativeRing : Ring, ICommutativeRing
	{
		/// <summary>
		/// Base class for all types of commutative rings.
		/// </summary>
		public CommutativeRing()
			: base()
		{
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left*(1/Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override sealed IRingElement RightDivide(IRingElement Left, IRingElement Right)
		{
			ICommutativeRingElement L = Left as ICommutativeRingElement;
			ICommutativeRingElement R = Right as ICommutativeRingElement;

			if (L == null || R == null)
				return base.RightDivide(Left, Right);
			else
				return this.Divide(L, R);
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ICommutativeRingElement Divide(ICommutativeRingElement Left, ICommutativeRingElement Right)
		{
			return this.Multiply(Left, Right.Invert()) as ICommutativeRingElement;
		}

		/// <summary>
		/// If the ring * operator is commutative or not.
		/// </summary>
		public override sealed bool IsCommutative
		{
			get
			{
				return true;
			}
		}

	}
}
