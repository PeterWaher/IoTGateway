using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Pseudo-field of double numbers, as an approximation of the field of real numbers.
	/// </summary>
	public sealed class DoubleNumbers : Field
	{
		private static readonly DoubleNumber zero = new DoubleNumber(0);
		private static readonly DoubleNumber one = new DoubleNumber(1);
		private static readonly int hashCode = typeof(DoubleNumbers).FullName.GetHashCode();

		/// <summary>
		/// Pseudo-field of double numbers, as an approximation of the field of real numbers.
		/// </summary>
		public DoubleNumbers()
		{
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return one; }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return zero; }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is DoubleNumber;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is DoubleNumbers;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
