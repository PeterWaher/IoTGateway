using System;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
    /// <summary>
    /// Boolean-valued number.
    /// </summary>
    public sealed class BooleanValue : FieldElement
    {
        private static readonly BooleanValues associatedField = new BooleanValues();

        private bool value;

        /// <summary>
        /// Boolean-valued number.
        /// </summary>
        /// <param name="Value">Boolean value.</param>
        public BooleanValue(bool Value)
        {
            this.value = Value;
        }

        /// <summary>
        /// Boolean value.
        /// </summary>
        public bool Value
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
            if (!(Element.AssociatedObjectValue is bool b))
                return null;
            else
                return new BooleanValue(this.value && b);
        }

        /// <summary>
        /// Inverts the element, if possible.
        /// </summary>
        /// <returns>Inverted element, or null if not possible.</returns>
        public override IRingElement Invert()
        {
            return new BooleanValue(this.value);
        }

        /// <summary>
        /// Tries to add an element to the current element.
        /// </summary>
        /// <param name="Element">Element to add.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override IAbelianGroupElement Add(IAbelianGroupElement Element)
        {
            if (!(Element.AssociatedObjectValue is bool b))
                return null;
            else
                return new BooleanValue(this.value ^ b);
        }

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
        {
            return new BooleanValue(this.value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is IElement E) || !(E.AssociatedObjectValue is bool b))
                return false;
            else
                return this.value == b;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Constant true value.
        /// </summary>
        public static readonly BooleanValue True = new BooleanValue(true);

        /// <summary>
        /// Constant false value.
        /// </summary>
        public static readonly BooleanValue False = new BooleanValue(false);

        /// <summary>
        /// Returns the zero element of the group.
        /// </summary>
        public override IAbelianGroupElement Zero
        {
            get { return False; }
        }

        /// <summary>
        /// Returns the identity element of the commutative ring with identity.
        /// </summary>
        public override ICommutativeRingWithIdentityElement One
        {
            get { return True; }
        }

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
			if (DesiredType == typeof(bool))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType.IsAssignableFrom(typeof(bool).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else
				return Expression.TryConvert(this.value, DesiredType, out Value);
        }
    }
}
