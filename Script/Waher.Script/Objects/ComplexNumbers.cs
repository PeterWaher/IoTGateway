using System.Numerics;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Pseudo-field of Complex numbers, as an approximation of the field of real numbers.
	/// </summary>
	public sealed class ComplexNumbers : Field
	{
		internal static readonly ComplexNumber zero = new ComplexNumber(Complex.Zero);
		internal static readonly ComplexNumber one = new ComplexNumber(Complex.One);
		private static readonly int hashCode = typeof(ComplexNumbers).FullName.GetHashCode();

		/// <summary>
		/// Pseudo-field of Complex numbers, as an approximation of the field of real numbers.
		/// </summary>
		public ComplexNumbers()
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
			return (Element is ComplexNumber) || (Element is DoubleNumber);
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ComplexNumbers;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

        /// <summary>
        /// Instance of the set of complex numbers.
        /// </summary>
        public static readonly ComplexNumbers Instance = new ComplexNumbers();

		/// <summary>
		/// <see cref="object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			return "ℂ";
		}
	}
}
