using System;
using System.Numerics;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Euclidian domain of integers.
	/// </summary>
	public sealed class Integers : EuclidianDomain
	{
		internal static readonly Integer zero = new Integer(BigInteger.Zero);
		internal static readonly Integer one = new Integer(BigInteger.One);
		private static readonly int hashCode = typeof(Integers).FullName.GetHashCode();

		/// <summary>
		/// Euclidian domain of integers.
		/// </summary>
		public Integers()
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
			return (Element is Integer) || (Element is DoubleNumber d && Math.Truncate(d.Value) == d.Value);
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Integers;
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
		public static readonly Integers Instance = new Integers();

		/// <summary>
		/// <see cref="object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			return "ℤ";
		}

		/// <summary>
		/// Divides the right ring element from the left one: Left/Right
		/// </summary>
		/// <param name="Left">Left element.</param>
		/// <param name="Right">Right element.</param>
		/// <param name="Remainder">Remainder.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IEuclidianDomainElement Divide(IEuclidianDomainElement Left, IEuclidianDomainElement Right, out IEuclidianDomainElement Remainder)
		{
			if (Left is Integer l && Right is Integer r)
			{
				BigInteger Result = BigInteger.DivRem(l.Value, r.Value, out BigInteger Residue);
				Remainder = new Integer(Residue);
				return new Integer(Result);
			}
			else
			{
				Remainder = null;
				return null;
			}
		}
	}
}
