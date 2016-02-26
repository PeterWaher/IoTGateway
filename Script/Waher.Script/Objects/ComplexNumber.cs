using System;
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

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
            if (DesiredType.IsAssignableFrom(typeof(Complex)))
            {
                Value = this.value;
                return true;
            }
            else if (this.value.Imaginary == 0)
            {
                double d = this.value.Real;

                switch (Type.GetTypeCode(DesiredType))
                {
                    case TypeCode.Byte:
                        if (d >= byte.MinValue && d <= byte.MaxValue)
                        {
                            Value = (byte)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.Decimal:
                        Value = (decimal)d;
                        return true;

                    case TypeCode.Double:
                        Value = (double)d;
                        return true;

                    case TypeCode.Int16:
                        if (d >= short.MinValue && d <= short.MaxValue)
                        {
                            Value = (short)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.Int32:
                        if (d >= int.MinValue && d <= int.MaxValue)
                        {
                            Value = (int)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.Int64:
                        if (d >= long.MinValue && d <= long.MaxValue)
                        {
                            Value = (long)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.SByte:
                        if (d >= sbyte.MinValue && d <= sbyte.MaxValue)
                        {
                            Value = (sbyte)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.Single:
                        Value = (float)d;
                        return true;

                    case TypeCode.UInt16:
                        if (d >= ushort.MinValue && d <= ushort.MaxValue)
                        {
                            Value = (ushort)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.UInt32:
                        if (d >= uint.MinValue && d <= uint.MaxValue)
                        {
                            Value = (uint)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.UInt64:
                        if (d >= ulong.MinValue && d <= ulong.MaxValue)
                        {
                            Value = (ulong)d;
                            return true;
                        }
                        else
                            break;

                    case TypeCode.Object:
                        if (DesiredType==typeof(ComplexNumber))
                        {
                            Value = this;
                            return true;
                        }
                        break;
                }
            }

            Value = null;
            return false;
        }
    }
}
