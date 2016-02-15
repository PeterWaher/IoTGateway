using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of rings.
	/// </summary>
	public abstract class Ring : AbelianGroup
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
		public virtual RingElement Multiply(RingElement Left, RingElement Right)
		{
			RingElement Result;

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
		public virtual RingElement RightDivide(RingElement Left, RingElement Right)
		{
			RingElement Inverse = Right.Invert();
			if (Inverse == null)
				return null;
			else
				return this.Multiply(Left, Inverse) as RingElement;
		}

		/// <summary>
		/// Divides the left ring element from the right one: (1/Left)*Right.
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual RingElement LeftDivide(RingElement Left, RingElement Right)
		{
			RingElement Inverse = Left.Invert();
			if (Inverse == null)
				return null;
			else
				return this.Multiply(Inverse, Right) as RingElement;
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
