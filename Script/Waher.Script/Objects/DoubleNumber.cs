using System;
using System.Collections.Generic;
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

        private static readonly DoubleNumbers associatedField = new DoubleNumbers();

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
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
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
			get { return this.value; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			DoubleNumber E = Element as DoubleNumber;
			if (E == null)
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
			DoubleNumber E = Element as DoubleNumber;
			if (E == null)
				return null;
			else
				return new DoubleNumber(this.value + E.value);
		}

		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new DoubleNumber(-this.value);
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			DoubleNumber E = obj as DoubleNumber;
			if (E == null)
				return false;
			else
				return this.value == E.value;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
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
	}
}
