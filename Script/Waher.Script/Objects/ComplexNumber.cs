﻿using System;
using System.Numerics;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Complex-valued number.
	/// </summary>
	public sealed class ComplexNumber : FieldElement
	{
		private static readonly ComplexNumbers associatedField = new ComplexNumbers();

		private Complex value;

		/// <summary>
		/// Complex-valued number.
		/// </summary>
		/// <param name="Value">Complex value.</param>
		public ComplexNumber(Complex Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Complex-valued number.
		/// </summary>
		/// <param name="Re">Real part.</param>
		/// <param name="Im">Imaginary part.</param>
		public ComplexNumber(double Re, double Im)
		{
			this.value = new Complex(Re, Im);
		}

		/// <summary>
		/// Complex value.
		/// </summary>
		public Complex Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Expression.ToString(this.value);
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
		public override object AssociatedObjectValue => this.value;

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			object Obj = Element.AssociatedObjectValue;

			if (Obj is Complex z)
				return new ComplexNumber(this.value * z);
			else if (Obj is double d)
				return new ComplexNumber(this.value * d);
			else
				return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			return new ComplexNumber(1.0 / this.value);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			object Obj = Element.AssociatedObjectValue;

			if (Obj is Complex z)
				return new ComplexNumber(this.value + z);
			else if (Obj is double d)
				return new ComplexNumber(this.value + d);
			else
				return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new ComplexNumber(-this.value);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is IElement E))
				return false;

			object Obj = E.AssociatedObjectValue;

			if (Obj is Complex z)
				return this.value == z;
			else if (Obj is double d)
				return this.value == d;
			else
				return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return ComplexNumbers.zero; }
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return ComplexNumbers.one; }
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType.IsAssignableFrom(typeof(Complex).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else if (this.value.Imaginary == 0)
			{
				double d = this.value.Real;

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
				else if (DesiredType.IsAssignableFrom(typeof(Complex).GetTypeInfo()))
				{
					Value = this.value;
					return true;
				}
				else if (DesiredType == typeof(ComplexNumber))
				{
					Value = this;
					return true;
				}
			}

			return Expression.TryConvert(this.value, DesiredType, out Value);
		}
	}
}
