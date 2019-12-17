using System;
using System.Numerics;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Field of rational numbers.
	/// </summary>
	public sealed class RationalNumbers : Field, IOrderedSet
	{
		internal static readonly RationalNumber zero = new RationalNumber(BigInteger.Zero);
		internal static readonly RationalNumber one = new RationalNumber(BigInteger.One);
		private static readonly int hashCode = typeof(RationalNumbers).FullName.GetHashCode();

		/// <summary>
		/// Field of rational numbers.
		/// </summary>
		public RationalNumbers()
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
			return Element is RationalNumber || Element is Integer || Element is DoubleNumber;
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is RationalNumbers;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Instance of the set of integers.
		/// </summary>
		public static readonly RationalNumbers Instance = new RationalNumbers();

		/// <summary>
		/// <see cref="object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			return "ℚ";
		}

		/// <summary>
		/// Compares two rational numbers.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public static int CompareNumbers(IElement x, IElement y)
		{
			BigInteger n1, d1;
			BigInteger n2, d2;

			if (x is RationalNumber q1)
			{
				n1 = q1.Numerator;
				d1 = q1.Denominator;
			}
			else if (x is Integer i1)
			{
				n1 = i1.Value;
				d1 = BigInteger.One;
			}
			else
			{
				n1 = BigInteger.Zero;
				d1 = BigInteger.One;
			}

			if (y is RationalNumber q2)
			{
				n2 = q2.Numerator;
				d2 = q2.Denominator;
			}
			else if (y is Integer i2)
			{
				n2 = i2.Value;
				d2 = BigInteger.One;
			}
			else
			{
				n2 = BigInteger.Zero;
				d2 = BigInteger.One;
			}

			BigInteger l = n1 * d2;
			BigInteger r = n2 * d1;

			return l.CompareTo(r);
		}

		/// <summary>
		/// Compares two rational numbers.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			return CompareNumbers(x, y);
		}
	}
}
