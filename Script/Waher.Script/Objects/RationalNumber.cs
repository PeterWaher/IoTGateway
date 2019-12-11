using System;
using System.Numerics;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Rational Number.
	/// </summary>
	public sealed class RationalNumber : FieldElement
	{
		private static readonly RationalNumbers associatedField = new RationalNumbers();

		private BigInteger numerator;
		private BigInteger denominator;

		/// <summary>
		/// Rational Number.
		/// </summary>
		/// <param name="Numerator">Numerator</param>
		/// <param name="Denominator">Denominator</param>
		public RationalNumber(BigInteger Numerator, BigInteger Denominator)
		{
			if (Denominator.IsZero)
				throw new DivideByZeroException();

			this.numerator = Numerator;
			this.denominator = Denominator;
		}

		/// <summary>
		/// RationalNumber-valued number.
		/// </summary>
		/// <param name="Number">Integer value.</param>
		public RationalNumber(BigInteger Number)
		{
			this.numerator = Number;
			this.denominator = BigInteger.One;
		}

		/// <summary>
		/// Numerator.
		/// </summary>
		public BigInteger Numerator
		{
			get { return this.numerator; }
			set { this.numerator = value; }
		}

		/// <summary>
		/// Denominator
		/// </summary>
		public BigInteger Denominator
		{
			get { return this.denominator; }
			set { this.denominator = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "(" + Expression.ToString(this.numerator) + "/" +
				Expression.ToString(this.denominator) + ")";
		}

		/// <summary>
		/// Associated Field.
		/// </summary>
		public override IField AssociatedField
		{
			get { return associatedField; }
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get { return this; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			if (Element is RationalNumber Q)
				return this * Q;

			if (Element is Integer i)
				return this * i.Value;

			if (Element is DoubleNumber D)
				return new DoubleNumber(this.ToDouble() * D.Value);

			if (Element is ComplexNumber z)
				return new ComplexNumber(this.ToDouble() * z.Value);

			return null;
		}

		/// <summary>
		/// Converts rational number to a double-precision floating-point number.
		/// </summary>
		/// <returns>Double-precision floating-point number.</returns>
		public double ToDouble()
		{
			return ((double)this.numerator) / ((double)this.denominator);
		}

		/// <summary>
		/// Multiplies a rational number with another rational number.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Product.</returns>
		public static RationalNumber operator *(RationalNumber Left, RationalNumber Right)
		{
			if (Left.numerator.IsZero || Right.numerator.IsZero)
				return RationalNumbers.zero;

			BigInteger gcd1 = BigInteger.GreatestCommonDivisor(Left.numerator, Right.denominator);
			BigInteger gcd2 = BigInteger.GreatestCommonDivisor(Right.numerator, Left.denominator);

			BigInteger n = BigInteger.Divide(Left.numerator, gcd1) * BigInteger.Divide(Right.numerator, gcd2);
			BigInteger d = BigInteger.Divide(Left.denominator, gcd2) * BigInteger.Divide(Right.denominator, gcd1);

			return new RationalNumber(n, d);
		}

		/// <summary>
		/// Multiplies a rational number with a big integer.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Product.</returns>
		public static RationalNumber operator *(RationalNumber Left, BigInteger Right)
		{
			BigInteger gcd2 = BigInteger.GreatestCommonDivisor(Right, Left.denominator);

			BigInteger n = Left.numerator * BigInteger.Divide(Right, gcd2);
			BigInteger d = BigInteger.Divide(Left.denominator, gcd2);

			return new RationalNumber(n, d);
		}

		/// <summary>
		/// Multiplies a big integer with a rational number.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Product.</returns>
		public static RationalNumber operator *(BigInteger Left, RationalNumber Right)
		{
			return Right * Left;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			if (this.numerator.IsZero)
				throw new DivideByZeroException();

			return new RationalNumber(this.denominator, this.numerator);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (Element is RationalNumber Q)
				return this + Q;

			if (Element is Integer i)
				return this + i.Value;

			if (Element is DoubleNumber D)
				return new DoubleNumber(this.ToDouble() + D.Value);

			if (Element is ComplexNumber z)
				return new ComplexNumber(this.ToDouble() + z.Value);

			return null;
		}

		/// <summary>
		/// Adds a rational number with another rational number.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Sum.</returns>
		public static RationalNumber operator +(RationalNumber Left, RationalNumber Right)
		{
			BigInteger n = Left.numerator * Right.denominator + Right.numerator * Left.denominator;
			BigInteger d = Right.denominator * Left.denominator;

			BigInteger gcd = BigInteger.GreatestCommonDivisor(n, d);

			return new RationalNumber(BigInteger.Divide(n, gcd), BigInteger.Divide(d, gcd));
		}

		/// <summary>
		/// Adds a rational number with a big integer.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Sum.</returns>
		public static RationalNumber operator +(RationalNumber Left, BigInteger Right)
		{
			BigInteger n = Left.numerator + Right * Left.denominator;
			BigInteger gcd = BigInteger.GreatestCommonDivisor(n, Left.denominator);

			return new RationalNumber(BigInteger.Divide(n, gcd), BigInteger.Divide(Left.denominator, gcd));
		}

		/// <summary>
		/// Adds a big integer with a rational number.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <returns>Sum.</returns>
		public static RationalNumber operator +(BigInteger Left, RationalNumber Right)
		{
			return Right + Left;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new RationalNumber(-this.numerator, this.denominator);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is RationalNumber E)
			{
				if (this.numerator.IsZero)
					return E.numerator.IsZero;
				else
					return this.numerator == E.numerator && this.denominator == E.denominator;
			}

			if (obj is DoubleNumber D)
				return this.ToDouble() == D.Value;

			if (obj is ComplexNumber z)
				return this.ToDouble() == z.Value;

			return false;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.numerator.GetHashCode();
			Result ^= Result << 5 ^ this.denominator.GetHashCode();
			return Result;
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return RationalNumbers.zero; }
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return RationalNumbers.one; }
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(RationalNumber).GetTypeInfo()))
			{
				Value = this;
				return true;
			}
			else
			{
				double d = this.ToDouble();

				if (DesiredType == typeof(byte))
				{
					if (d >= byte.MinValue && d <= byte.MaxValue)
					{
						Value = (byte)d;
						return true;
					}
				}
				else if (DesiredType == typeof(decimal))
				{
					Value = (decimal)d;
					return true;
				}
				else if (DesiredType == typeof(double))
				{
					Value = (double)d;
					return true;
				}
				else if (DesiredType == typeof(short))
				{
					if (d >= short.MinValue && d <= short.MaxValue)
					{
						Value = (short)d;
						return true;
					}
				}
				else if (DesiredType == typeof(int))
				{
					if (d >= int.MinValue && d <= int.MaxValue)
					{
						Value = (int)d;
						return true;
					}
				}
				else if (DesiredType == typeof(long))
				{
					if (d >= long.MinValue && d <= long.MaxValue)
					{
						Value = (long)d;
						return true;
					}
				}
				else if (DesiredType == typeof(sbyte))
				{
					if (d >= sbyte.MinValue && d <= sbyte.MaxValue)
					{
						Value = (sbyte)d;
						return true;
					}
				}
				else if (DesiredType == typeof(float))
				{
					Value = (float)d;
					return true;
				}
				else if (DesiredType == typeof(ushort))
				{
					if (d >= ushort.MinValue && d <= ushort.MaxValue)
					{
						Value = (ushort)d;
						return true;
					}
				}
				else if (DesiredType == typeof(uint))
				{
					if (d >= uint.MinValue && d <= uint.MaxValue)
					{
						Value = (uint)d;
						return true;
					}
				}
				else if (DesiredType == typeof(ulong))
				{
					if (d >= ulong.MinValue && d <= ulong.MaxValue)
					{
						Value = (ulong)d;
						return true;
					}
				}
			}

			return Expression.TryConvert(this, DesiredType, out Value);
		}
	}
}
