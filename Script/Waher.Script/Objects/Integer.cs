using System;
using System.Numerics;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Integer-valued number.
	/// </summary>
	public sealed class Integer : EuclidianDomainElement
	{
		private static readonly Integers associatedEuclidianDomain = new Integers();

		private BigInteger value;

		/// <summary>
		/// Integer-valued number.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public Integer(BigInteger Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// BigInteger-valued number.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public Integer(int Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// BigInteger-valued number.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public Integer(uint Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// BigInteger-valued number.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public Integer(long Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// BigInteger-valued number.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public Integer(ulong Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// BigInteger value.
		/// </summary>
		public BigInteger Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return Expression.ToString(this.value);
		}

		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		public override IEuclidianDomain AssociatedEuclidianDomain
		{
			get { return associatedEuclidianDomain; }
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			if (Element is Integer E)
				return new Integer(this.value * E.value);

			if (Element is DoubleNumber D)
				return new DoubleNumber((double)this.value * D.Value);

			if (Element is ComplexNumber z)
				return new ComplexNumber((double)this.value * z.Value);

			return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			if (this.value.IsOne)
				return new Integer(this.value);
			else if (this.value == -1)
				return new Integer(this.value);
			else
				return new DoubleNumber(1.0 / (double)this.value);  // TODO: Rational numbers
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (Element is Integer E)
				return new Integer(this.value + E.value);

			if (Element is DoubleNumber D)
				return new DoubleNumber((double)this.value + D.Value);

			if (Element is ComplexNumber z)
				return new ComplexNumber((double)this.value + z.Value);

			return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new Integer(-this.value);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is Integer E)
				return this.value == E.value;

			if (obj is DoubleNumber D)
				return (double)this.value == D.Value;

			if (obj is ComplexNumber z)
				return (double)this.value == z.Value;

			return false;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return Integers.zero; }
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return Integers.one; }
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(BigInteger).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType == typeof(byte))
			{
				if (this.value >= byte.MinValue && this.value <= byte.MaxValue)
				{
					Value = (byte)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(decimal))
			{
				Value = (decimal)this.value;
				return true;
			}
			else if (DesiredType == typeof(double))
			{
				Value = (double)this.value;
				return true;
			}
			else if (DesiredType == typeof(short))
			{
				if (this.value >= short.MinValue && this.value <= short.MaxValue)
				{
					Value = (short)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(int))
			{
				if (this.value >= int.MinValue && this.value <= int.MaxValue)
				{
					Value = (int)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(long))
			{
				if (this.value >= long.MinValue && this.value <= long.MaxValue)
				{
					Value = (long)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(sbyte))
			{
				if (this.value >= sbyte.MinValue && this.value <= sbyte.MaxValue)
				{
					Value = (sbyte)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(float))
			{
				Value = (float)this.value;
				return true;
			}
			else if (DesiredType == typeof(ushort))
			{
				if (this.value >= ushort.MinValue && this.value <= ushort.MaxValue)
				{
					Value = (ushort)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(uint))
			{
				if (this.value >= uint.MinValue && this.value <= uint.MaxValue)
				{
					Value = (uint)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(ulong))
			{
				if (this.value >= ulong.MinValue && this.value <= ulong.MaxValue)
				{
					Value = (ulong)this.value;
					return true;
				}
			}
			else if (DesiredType == typeof(Integer))
			{
				Value = this;
				return true;
			}

			return Expression.TryConvert(this.value, DesiredType, out Value);
		}
	}
}
