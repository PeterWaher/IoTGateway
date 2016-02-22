using System.Numerics;
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
		/// Complex value.
		/// </summary>
		public Complex Value
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
			ComplexNumber E = Element as ComplexNumber;
            if (E == null)
            {
                DoubleNumber D = Element as DoubleNumber;
                if (D == null)
                    return null;
                else
                    return new ComplexNumber(this.value * D.Value);
            }
            else
                return new ComplexNumber(this.value * E.value);
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
			ComplexNumber E = Element as ComplexNumber;
            if (E == null)
            {
                DoubleNumber D = Element as DoubleNumber;
                if (D == null)
                    return null;
                else
                    return new ComplexNumber(this.value + D.Value);
            }
            else
                return new ComplexNumber(this.value + E.value);
		}

		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new ComplexNumber(-this.value);
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			ComplexNumber E = obj as ComplexNumber;
            if (E == null)
            {
                DoubleNumber D = obj as DoubleNumber;
                if (D == null)
                    return false;
                else
                    return this.value == D.Value;
            }
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
			get { return ComplexNumbers.zero; }
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return ComplexNumbers.one; }
		}
	}
}
