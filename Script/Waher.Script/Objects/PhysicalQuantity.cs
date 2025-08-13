﻿using System;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Units;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Physical quantity.
	/// </summary>
	public sealed class PhysicalQuantity : FieldElement, IComparable, IPhysicalQuantity
	{
		/// <summary>
		/// 0
		/// </summary>
		public static readonly PhysicalQuantity ZeroElement = new PhysicalQuantity(0, Unit.Empty);

		/// <summary>
		/// 1
		/// </summary>
		public static readonly PhysicalQuantity OneElement = new PhysicalQuantity(1, Unit.Empty);

		private static readonly PhysicalQuantities associatedField = new PhysicalQuantities();

		private double magnitude;
		private Unit unit;

		/// <summary>
		/// Physical quantity.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="Unit">Unit</param>
		public PhysicalQuantity(double Magnitude, Unit Unit)
		{
			this.magnitude = Magnitude;
			this.unit = Unit;
		}

		/// <summary>
		/// Magnitude
		/// </summary>
		public double Magnitude
		{
			get => this.magnitude;
			set => this.magnitude = value;
		}

		/// <summary>
		/// Unit
		/// </summary>
		public Unit Unit
		{
			get => this.unit;
			set => this.unit = value;
		}

		/// <summary>
		/// Converts underlying object to a physical quantity.
		/// </summary>
		/// <returns>Physical quantity</returns>
		public PhysicalQuantity ToPhysicalQuantity() => this;

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.unit.IsEmpty)
				return Expression.ToString(this.magnitude);
			else
				return Expression.ToString(this.magnitude) + " " + this.unit.ToString();
		}

		/// <summary>
		/// Associated Field.
		/// </summary>
		public override IField AssociatedField => associatedField;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this;

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			if (Element.AssociatedObjectValue is IPhysicalQuantity E)
			{
				PhysicalQuantity Q = E.ToPhysicalQuantity();
				Unit Unit = Unit.Multiply(this.unit, Q.unit, out int ResidueExponential);
				double Magnitude = this.magnitude * Q.magnitude;
				if (ResidueExponential != 0)
					Magnitude *= Math.Pow(10, ResidueExponential);

				return new PhysicalQuantity(Magnitude, Unit);
			}
			else if (Element.AssociatedObjectValue is double d)
				return new PhysicalQuantity(this.magnitude * d, this.unit);
			else
				return null;
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			Unit Unit = this.unit.Invert(out int ResidueExponential);
			double Magnitude = 1 / this.magnitude;
			if (ResidueExponential != 0)
				Magnitude *= Math.Pow(10, ResidueExponential);

			return new PhysicalQuantity(Magnitude, Unit);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (Element.AssociatedObjectValue is IPhysicalQuantity E)
			{
				PhysicalQuantity Q = E.ToPhysicalQuantity();
				if (Unit.TryConvert(Q.magnitude, Q.unit, this.unit, out double d))
					return new PhysicalQuantity(this.magnitude + d, this.unit);
			}
			else if (Element.AssociatedObjectValue is double d)
				return new PhysicalQuantity(this.magnitude + d, this.unit);
				
			return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new PhysicalQuantity(-this.magnitude, this.unit);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is PhysicalQuantity E))
				return false;

			if (this.unit.Equals(E.unit, true))
				return this.magnitude == E.magnitude;
			else
			{
				double m1 = this.magnitude;
				this.unit.ToReferenceUnits(ref m1);

				double m2 = E.magnitude;
				E.unit.ToReferenceUnits(ref m2);

				return m1 == m2;
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.magnitude.GetHashCode();
			Result ^= Result << 5 ^ this.unit.GetHashCode();
			return Result;
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero => ZeroElement;

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One => OneElement;

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
				if (this.magnitude >= byte.MinValue && this.magnitude <= byte.MaxValue)
				{
					Value = (byte)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(decimal))
			{
				Value = (decimal)this.magnitude;
				return true;
			}
			else if (DesiredType == typeof(double))
			{
				Value = (double)this.magnitude;
				return true;
			}
			else if (DesiredType == typeof(short))
			{
				if (this.magnitude >= short.MinValue && this.magnitude <= short.MaxValue)
				{
					Value = (short)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(int))
			{
				if (this.magnitude >= int.MinValue && this.magnitude <= int.MaxValue)
				{
					Value = (int)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(long))
			{
				if (this.magnitude >= long.MinValue && this.magnitude <= long.MaxValue)
				{
					Value = (long)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(sbyte))
			{
				if (this.magnitude >= sbyte.MinValue && this.magnitude <= sbyte.MaxValue)
				{
					Value = (sbyte)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(float))
			{
				Value = (float)this.magnitude;
				return true;
			}
			else if (DesiredType == typeof(ushort))
			{
				if (this.magnitude >= ushort.MinValue && this.magnitude <= ushort.MaxValue)
				{
					Value = (ushort)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(uint))
			{
				if (this.magnitude >= uint.MinValue && this.magnitude <= uint.MaxValue)
				{
					Value = (uint)this.magnitude;
					return true;
				}
			}
			else if (DesiredType == typeof(ulong))
			{
				if (this.magnitude >= ulong.MinValue && this.magnitude <= ulong.MaxValue)
				{
					Value = (ulong)this.magnitude;
					return true;
				}
			}
			else if (DesiredType.IsAssignableFrom(typeof(PhysicalQuantity).GetTypeInfo()))
			{
				Value = this;
				return true;
			}

			return Expression.TryConvert(this, DesiredType, out Value);
		}

		/// <summary>
		/// <see cref="IComparable.CompareTo"/>
		/// </summary>
		public int CompareTo(object obj)
		{
			if (!(obj is IPhysicalQuantity PQ))
				throw new ScriptException("Values not comparable.");

			PhysicalQuantity Q = PQ.ToPhysicalQuantity();

			if (this.unit.Equals(Q.unit, true))
				return this.magnitude.CompareTo(Q.magnitude);

			if (!Unit.TryConvert(Q.magnitude, Q.unit, this.unit, out double d))
				throw new ScriptException("Values not comparable.");

			return this.magnitude.CompareTo(d);
		}

		/// <summary>
		/// Tries to parse a string to a physical quantity.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Value">Parsed Value</param>
		/// <returns>If the string could be parsed into a physical quantity.</returns>
		public static bool TryParse(string s, out PhysicalQuantity Value)
		{
			int i = s.Length - 1;

			while (i >= 0 && char.IsWhiteSpace(s[i]))
				i--;

			int j = i;

			while (i >= 0 && !char.IsWhiteSpace(s[i]))
				i--;

			if (i < 0 ||
				!Unit.TryParse(s.Substring(i + 1, j - i), out Unit ParsedUnit) ||
				!double.TryParse(s.Substring(0, i).Trim().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."), out double ParsedValue))
			{
				Value = null;
				return false;
			}

			Value = new PhysicalQuantity(ParsedValue, ParsedUnit);

			return true;
		}
	}
}
