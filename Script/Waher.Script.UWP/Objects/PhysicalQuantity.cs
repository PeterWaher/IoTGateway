using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Units;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Physical quantity.
	/// </summary>
	public sealed class PhysicalQuantity : FieldElement, IComparable
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
			get { return this.magnitude; }
		}

		/// <summary>
		/// Unit
		/// </summary>
		public Unit Unit
		{
			get { return this.unit; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
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
			PhysicalQuantity E = Element as PhysicalQuantity;
			if (E == null)
			{
				DoubleNumber n = Element as DoubleNumber;
				if (n != null)
					return new PhysicalQuantity(this.magnitude * n.Value, this.unit);
				else
					return null;
			}
			else
			{
				int ResidueExponential;
				Unit Unit = Unit.Multiply(this.unit, E.unit, out ResidueExponential);
				double Magnitude = this.magnitude * E.magnitude;
				if (ResidueExponential != 0)
					Magnitude *= Math.Pow(10, ResidueExponential);

				return new PhysicalQuantity(Magnitude, Unit);
			}
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			int ResidueExponential;
			Unit Unit = this.unit.Invert(out ResidueExponential);
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
			PhysicalQuantity E = Element as PhysicalQuantity;
			double d;

			if (E == null)
			{
				DoubleNumber n = Element as DoubleNumber;
				if (n != null)
					return new PhysicalQuantity(this.magnitude + n.Value, this.unit);
				else
					return null;
			}
			else if (Unit.TryConvert(E.magnitude, E.unit, this.unit, out d))
				return new PhysicalQuantity(this.magnitude + d, this.unit);
			else
				return null;
		}

		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new PhysicalQuantity(-this.magnitude, this.unit);
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			PhysicalQuantity E = obj as PhysicalQuantity;
			if (E == null)
				return false;
			if (this.unit.Equals(E.unit))
				return this.magnitude == E.magnitude;
			else
			{
				double m1 = this.magnitude;
				Unit U1 = this.unit.ToReferenceUnits(ref m1);

				double m2 = E.magnitude;
				Unit U2 = E.unit.ToReferenceUnits(ref m2);

				return m1 == m2;
			}
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.magnitude.GetHashCode() ^ this.unit.GetHashCode();
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
#if WINDOWS_UWP
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
			else if (DesiredType == typeof(PhysicalQuantity))
			{
				Value = this;
				return true;
			}
#else
			switch (Type.GetTypeCode(DesiredType))
			{
				case TypeCode.Byte:
					if (this.magnitude >= byte.MinValue && this.magnitude <= byte.MaxValue)
					{
						Value = (byte)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.Decimal:
					Value = (decimal)this.magnitude;
					return true;

				case TypeCode.Double:
					Value = (double)this.magnitude;
					return true;

				case TypeCode.Int16:
					if (this.magnitude >= short.MinValue && this.magnitude <= short.MaxValue)
					{
						Value = (short)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.Int32:
					if (this.magnitude >= int.MinValue && this.magnitude <= int.MaxValue)
					{
						Value = (int)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.Int64:
					if (this.magnitude >= long.MinValue && this.magnitude <= long.MaxValue)
					{
						Value = (long)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.SByte:
					if (this.magnitude >= sbyte.MinValue && this.magnitude <= sbyte.MaxValue)
					{
						Value = (sbyte)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.Single:
					Value = (float)this.magnitude;
					return true;

				case TypeCode.UInt16:
					if (this.magnitude >= ushort.MinValue && this.magnitude <= ushort.MaxValue)
					{
						Value = (ushort)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.UInt32:
					if (this.magnitude >= uint.MinValue && this.magnitude <= uint.MaxValue)
					{
						Value = (uint)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.UInt64:
					if (this.magnitude >= ulong.MinValue && this.magnitude <= ulong.MaxValue)
					{
						Value = (ulong)this.magnitude;
						return true;
					}
					else
						break;

				case TypeCode.Object:
					if (DesiredType.IsAssignableFrom(typeof(PhysicalQuantity)))
					{
						Value = this;
						return true;
					}
					break;
			}
#endif
			Value = null;
			return false;
		}

		/// <summary>
		/// <see cref="IComparable.CompareTo"/>
		/// </summary>
		public int CompareTo(object obj)
		{
			PhysicalQuantity Q = obj as PhysicalQuantity;
			if (Q == null)
				throw new ScriptException("Values not comparable.");

			if (this.unit.Equals(Q.unit))
				return this.magnitude.CompareTo(Q.magnitude);

			double d;

			if (!Unit.TryConvert(Q.magnitude, Q.unit, this.unit, out d))
				throw new ScriptException("Values not comparable.");

			return this.magnitude.CompareTo(d);
		}
	}
}
