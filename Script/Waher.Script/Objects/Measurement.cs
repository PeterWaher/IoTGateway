using System;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Units;
using System.Text;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Physical measurement
	/// </summary>
	public sealed class Measurement : FieldElement, IComparable, IPhysicalQuantity     // Not a proper field, as division is not the inversion of multiplication
	{
		/// <summary>
		/// 0
		/// </summary>
		public static readonly Measurement ZeroElement = new Measurement(0, Unit.Empty, 0);

		/// <summary>
		/// 1
		/// </summary>
		public static readonly Measurement OneElement = new Measurement(1, Unit.Empty, 1);

		private static readonly Measurements associatedField = new Measurements();

		private double magnitude;
		private double error;
		private Unit unit;

		/// <summary>
		/// Physical quantity.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="Unit">Unit</param>
		/// <param name="Error">Error</param>
		public Measurement(double Magnitude, Unit Unit, double Error)
		{
			this.magnitude = Magnitude;
			this.unit = Unit;
			this.error = Math.Abs(Error);
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
		/// Magnitude
		/// </summary>
		public double Error
		{
			get => this.error;
			set => this.error = value;
		}

		/// <summary>
		/// Estimate of measurement
		/// </summary>
		public PhysicalQuantity Estimate
		{
			get { return new PhysicalQuantity(this.magnitude, this.unit); }
		}

		/// <summary>
		/// Estimate of measurement
		/// </summary>
		public PhysicalQuantity Max
		{
			get { return new PhysicalQuantity(this.magnitude + this.error, this.unit); }
		}

		/// <summary>
		/// Estimate of measurement
		/// </summary>
		public PhysicalQuantity Min
		{
			get { return new PhysicalQuantity(this.magnitude - this.error, this.unit); }
		}

		/// <summary>
		/// Converts underlying object to a physical quantity.
		/// </summary>
		/// <returns>Physical quantity</returns>
		public PhysicalQuantity ToPhysicalQuantity() => this.Estimate;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Expression.ToString(this.magnitude));
			if (!this.unit.IsEmpty)
			{
				sb.Append(' ');
				sb.Append(this.unit.ToString());
			}

			if (this.error != 0)
			{
				sb.Append(" ± ");
				sb.Append(Expression.ToString(this.error));

				if (!this.unit.IsEmpty)
				{
					sb.Append(' ');
					sb.Append(this.unit.ToString());
				}
			}

			return sb.ToString();
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
			if (Element is Measurement E)
			{
				Unit Unit = Unit.Multiply(this.unit, E.unit, out int ResidueExponential);
				double Magnitude = this.magnitude * E.magnitude;
				if (ResidueExponential != 0)
					Magnitude *= Math.Pow(10, ResidueExponential);

				double Error1 = this.error / this.magnitude;
				double Error2 = E.error / E.magnitude;
				double Error = (Error1 + Error2) * Magnitude;

				return new Measurement(Magnitude, Unit, Error);
			}
			else if (Element.AssociatedObjectValue is double d)
				return new Measurement(this.magnitude * d, this.unit, this.error * d);
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

			double Error = this.error / this.magnitude * Magnitude;

			return new Measurement(Magnitude, Unit, Error);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			if (Element is Measurement E)
			{
				if (!Unit.TryConvert(E.magnitude, E.unit, this.unit, out double d))
					return null;

				double Magnitude = this.magnitude + d;

				if (!Unit.TryConvert(E.error, E.unit, this.unit, out d))
					return null;

				return new Measurement(Magnitude, this.unit, this.error + d);
			}
			else if (Element.AssociatedObjectValue is double d)
				return new Measurement(this.magnitude + d, this.unit, this.error);
			else
				return null;
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new Measurement(-this.magnitude, this.unit, this.error);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Measurement E))
				return false;

			if (this.unit.Equals(E.unit))
				return this.magnitude == E.magnitude && this.error == E.error;
			else
			{
				double m1 = this.magnitude;
				this.unit.ToReferenceUnits(ref m1);

				double m2 = E.magnitude;
				E.unit.ToReferenceUnits(ref m2);

				if (m1 != m2)
					return false;

				m1 = this.error;
				this.unit.ToReferenceUnits(ref m1);

				m2 = E.error;
				E.unit.ToReferenceUnits(ref m2);

				return m1 == m2;
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.magnitude.GetHashCode();
			Result ^= Result << 5 ^ this.unit.GetHashCode();
			Result ^= Result << 5 ^ this.error.GetHashCode();
			return Result;
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
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(Measurement).GetTypeInfo()))
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
			if (!(obj is Measurement Q))
				throw new ScriptException("Values not comparable.");

			int i;

			if (this.unit.Equals(Q.unit))
			{
				i = this.magnitude.CompareTo(Q.magnitude);
				if (i != 0)
					return i;

				return this.error.CompareTo(Q.error);
			}

			if (!Unit.TryConvert(Q.magnitude, Q.unit, this.unit, out double d))
				throw new ScriptException("Values not comparable.");

			i = this.magnitude.CompareTo(d);
			if (i != 0)
				return i;

			if (!Unit.TryConvert(Q.error, Q.unit, this.unit, out d))
				throw new ScriptException("Values not comparable.");

			return this.error.CompareTo(d);
		}

		/// <summary>
		/// Tries to parse a string to a physical quantity.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Value">Parsed Value</param>
		/// <returns>If the string could be parsed into a physical quantity.</returns>
		public static bool TryParse(string s, out Measurement Value)
		{
			int i = s.IndexOf('±');
			if (i < 0)
				i = s.IndexOf("+-");

			if (i >= 0)
			{
				string s1 = s.Substring(0, i).Trim();
				string s2 = s.Substring(i + 1).Trim();

				if (PhysicalQuantity.TryParse(s1, out PhysicalQuantity Q) && PhysicalQuantity.TryParse(s2, out PhysicalQuantity E))
				{
					if (Unit.TryConvert(E.Magnitude, E.Unit, Q.Unit, out double Error))
					{
						Value = new Measurement(Q.Magnitude, Q.Unit, Error);
						return true;
					}
				}
				else if (Expression.TryParse(s1, out double d1) && Expression.TryParse(s2, out double d2))
				{
					Value = new Measurement(d1, Unit.Empty, d2);
					return true;
				}
			}

			Value = null;
			return false;
		}

	}
}
