using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// The field Z_2 of boolean numbers ([0]_2, 0 or false, and [1]_2, 1 or true).
	/// </summary>
	public sealed class BooleanValues : Field
	{
		private static readonly BooleanValue zero = new BooleanValue(false);
		private static readonly BooleanValue one = new BooleanValue(true);
		private static readonly int hashCode = typeof(BooleanValues).FullName.GetHashCode();

		/// <summary>
		/// Pseudo-field of double numbers, as an approximation of the field of real numbers.
		/// </summary>
		public BooleanValues()
		{
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override CommutativeRingWithIdentityElement One
		{
			get { return one; }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override AbelianGroupElement Zero
		{
			get { return zero; }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(Element Element)
		{
			return Element is BooleanValue;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is BooleanValues;
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
