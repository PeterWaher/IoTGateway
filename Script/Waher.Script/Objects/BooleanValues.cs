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
	public sealed class BooleanValues : Field, IOrderedSet
	{
		private static readonly int hashCode = typeof(BooleanValues).FullName.GetHashCode();

		/// <summary>
		/// The field Z_2 of boolean numbers ([0]_2, 0 or false, and [1]_2, 1 or true).
		/// </summary>
		public BooleanValues()
		{
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return BooleanValue.True; }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return BooleanValue.False; }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is BooleanValue;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is BooleanValues;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two double values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			BooleanValue b1 = (BooleanValue)x;
			BooleanValue b2 = (BooleanValue)y;

			return b1.Value.CompareTo(b2.Value);
		}
	}
}
