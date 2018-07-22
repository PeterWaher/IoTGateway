using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Double-valued number.
	/// </summary>
	public sealed class DoubleNumber : FieldElement
	{
		/// <summary>
		/// 0
		/// </summary>
		public static readonly DoubleNumber ZeroElement = new DoubleNumber(0);

		/// <summary>
		/// 1
		/// </summary>
		public static readonly DoubleNumber OneElement = new DoubleNumber(1);

		/// <summary>
		/// 2
		/// </summary>
		public static readonly DoubleNumber TwoElement = new DoubleNumber(2);

		/// <summary>
		/// 3
		/// </summary>
		public static readonly DoubleNumber ThreeElement = new DoubleNumber(3);

		private double value;

		/// <summary>
		/// Double-valued number.
		/// </summary>
		/// <param name="Value">Double value.</param>
		public DoubleNumber(double Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Double value.
		/// </summary>
		public double Value
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
		/// Associated Field.
		/// </summary>
		public override IField AssociatedField
		{
			get { return DoubleNumbers.Instance; }
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
			if (!(Element is DoubleNumber E))
				return null;
			else
				return new DoubleNumber(this.value * E.value);
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			return new DoubleNumber(1.0 / this.value);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (!(Element is DoubleNumber E))
				return null;
			else
				return new DoubleNumber(this.value + E.value);
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new DoubleNumber(-this.value);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is DoubleNumber E))
				return false;
			else
				return this.value == E.value;
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
			get { return ZeroElement; }
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return OneElement; }
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType == typeof(byte))
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
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(double).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType == typeof(DoubleNumber))
			{
				Value = this;
				return true;
			}

			Value = null;
			return false;
		}
	}
}
