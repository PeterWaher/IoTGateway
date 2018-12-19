using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of rings.
	/// </summary>
	public abstract class Ring : AbelianGroup, IRing
	{
		/// <summary>
		/// Base class for all types of rings.
		/// </summary>
		public Ring()
			: base()
		{
		}

		/// <summary>
		/// Multiplies two ring elements, if possible.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IRingElement Multiply(IRingElement Left, IRingElement Right)
		{
			IRingElement Result;

			Result = Left.MultiplyRight(Right);
			if (Result != null)
				return Result;

			Result = Right.MultiplyLeft(Left);
			if (Result != null)
				return Result;

			return null;
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left*(1/Right)
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IRingElement RightDivide(IRingElement Left, IRingElement Right)
		{
			IRingElement Inverse = Right.Invert();
			if (Inverse is null)
				return null;
			else
				return this.Multiply(Left, Inverse) as IRingElement;
		}

		/// <summary>
		/// Divides the left ring element from the right one: (1/Left)*Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IRingElement LeftDivide(IRingElement Left, IRingElement Right)
		{
			IRingElement Inverse = Left.Invert();
			if (Inverse is null)
				return null;
			else
				return this.Multiply(Inverse, Right) as IRingElement;
		}

		/// <summary>
		/// If the ring * operator is commutative or not.
		/// </summary>
		public abstract bool IsCommutative
		{
			get;
		}
	}
}
