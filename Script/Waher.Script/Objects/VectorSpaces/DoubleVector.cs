using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Objects.VectorSpaces
{
    /// <summary>
    /// Double-valued vector.
    /// </summary>
    public sealed class DoubleVector : VectorSpaceElement
    {
        private double[] values;
        private ICollection<IElement> elements;
        private int dimension;

        /// <summary>
        /// Double-valued vector.
        /// </summary>
        /// <param name="Values">Double values.</param>
        public DoubleVector(params double[] Values)
        {
            this.values = Values;
            this.elements = null;
            this.dimension = Values.Length;
        }

        /// <summary>
        /// Double-valued vector.
        /// </summary>
        /// <param name="Elements">Elements.</param>
        public DoubleVector(ICollection<IElement> Elements)
        {
            this.values = null;
            this.elements = Elements;
            this.dimension = Elements.Count;
        }

        /// <summary>
        /// Vector element values.
        /// </summary>
        public double[] Values
        {
            get
            {
                if (this.values == null)
                {
                    double[] v = new double[this.dimension];
                    int i = 0;

                    foreach (DoubleNumber Element in this.elements)
                        v[i++] = Element.Value;

                    this.values = v;
                }

                return this.values;
            }
        }

        /// <summary>
        /// Vector elements.
        /// </summary>
        public ICollection<IElement> Elements
        {
            get
            {
                if (this.elements == null)
                {
                    int i;
                    IElement[] v = new IElement[this.dimension];

                    for (i = 0; i < this.dimension; i++)
                        v[i] = new DoubleNumber(this.values[i]);

                    this.elements = v;
                }

                return this.elements;
            }
        }

        /// <summary>
        /// Dimension of vector.
        /// </summary>
        public override int Dimension
        {
            get { return this.dimension; }
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = null;

            foreach (double d in this.Values)
            {
                if (sb == null)
                    sb = new StringBuilder("[");
                else
                    sb.Append(", ");

                sb.Append(Expression.ToString(d));
            }

            if (sb == null)
                return "[]";
            else
            {
                sb.Append(']');
                return sb.ToString();
            }
        }

        /// <summary>
        /// Associated Right-VectorSpace.
        /// </summary>
        public override IVectorSpace AssociatedVectorSpace
        {
            get
            {
                if (this.associatedVectorSpace == null)
                    this.associatedVectorSpace = new DoubleVectors(this.dimension);

                return this.associatedVectorSpace;
            }
        }

        private DoubleVectors associatedVectorSpace = null;

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get { return this.Values; }
        }

        /// <summary>
        /// Tries to multiply a scalar to the current element.
        /// </summary>
        /// <param name="Scalar">Scalar to multiply.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override IVectorSpaceElement MultiplyScalar(IFieldElement Scalar)
        {
            DoubleNumber DoubleNumber = Scalar as DoubleNumber;
            if (DoubleNumber == null)
                return null;

            double d = DoubleNumber.Value;
            int i;
            double[] Values = this.Values;
            double[] v = new double[this.dimension];

            for (i = 0; i < this.dimension; i++)
                v[i] = d * Values[i];

            return new DoubleVector(v);
        }

        /// <summary>
        /// Tries to add an element to the current element.
        /// </summary>
        /// <param name="Element">Element to add.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override IAbelianGroupElement Add(IAbelianGroupElement Element)
        {
            DoubleVector DoubleVector = Element as DoubleVector;
            if (DoubleVector == null)
                return null;

            int i;
            if (DoubleVector.dimension != this.dimension)
                return null;

            double[] Values = this.Values;
            double[] Values2 = DoubleVector.Values;
            double[] v = new double[this.dimension];
            for (i = 0; i < this.dimension; i++)
                v[i] = Values[i] + Values2[i];

            return new DoubleVector(v);
        }

        /// <summary>
        /// Negates the element.
        /// </summary>
        /// <returns>Negation of current element.</returns>
        public override IGroupElement Negate()
        {
            int i;
            double[] v = new double[this.dimension];
            double[] Values = this.Values;
            for (i = 0; i < this.dimension; i++)
                v[i] = -Values[i];

            return new DoubleVector(v);
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            DoubleVector DoubleVector = obj as DoubleVector;
            if (DoubleVector == null)
                return false;

            int i;
            if (DoubleVector.dimension != this.dimension)
                return false;

            double[] Values = this.Values;
            double[] Values2 = DoubleVector.Values;
            for (i = 0; i < this.dimension; i++)
            {
                if (Values[i] != Values2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates a hash code of the element.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            double[] Values = this.Values;
            int Result = 0;
            int i;

            for (i = 0; i < this.dimension; i++)
                Result ^= this.values[i].GetHashCode();

            return Result;
        }

        /// <summary>
        /// If the element represents a scalar value.
        /// </summary>
        public override bool IsScalar
        {
            get { return false; }
        }

        /// <summary>
        /// An enumeration of child elements. If the element is a scalar, this property will return null.
        /// </summary>
        public override ICollection<IElement> ChildElements
        {
            get { return this.Elements; }
        }

        /// <summary>
        /// Encapsulates a set of elements into a similar structure as that provided by the current element.
        /// </summary>
        /// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
        /// <param name="Node">Script node from where the encapsulation is done.</param>
        /// <returns>Encapsulated object of similar type as the current object.</returns>
        public override IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
        {
            return VectorDefinition.Encapsulate(Elements, true, Node);
        }

        /// <summary>
        /// Returns the zero element of the group.
        /// </summary>
        public override IAbelianGroupElement Zero
        {
            get
            {
                if (this.zero == null)
                    this.zero = new DoubleVector(new double[this.dimension]);

                return this.zero;
            }
        }

        private DoubleVector zero = null;

        /// <summary>
        /// Gets an element of the vector.
        /// </summary>
        /// <param name="Index">Zero-based index into the vector.</param>
        /// <returns>Vector element.</returns>
        public override IElement GetElement(int Index)
        {
            if (Index < 0 || Index >= this.dimension)
                throw new ScriptException("Index out of bounds.");

            double[] V = this.Values;

            return new DoubleNumber(V[Index]);
        }

        /// <summary>
        /// Sets an element in the vector.
        /// </summary>
        /// <param name="Index">Index.</param>
        /// <param name="Value">Element to set.</param>
        public override void SetElement(int Index, IElement Value)
        {
            if (Index < 0 || Index >= this.dimension)
                throw new ScriptException("Index out of bounds.");

            DoubleNumber V = Value as DoubleNumber;
            if (V == null)
                throw new ScriptException("Elements in a double vector are required to be double values.");

            double[] Values = this.Values;
            this.elements = null;

            Values[Index] = V.Value;
        }

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
            if (DesiredType == typeof(double[]))
            {
                Value = this.Values;
                return true;
            }
            else if (DesiredType.IsAssignableFrom(typeof(DoubleVector)))
            {
                Value = this;
                return true;
            }
            else if (DesiredType.IsArray)
            {
                Type ElementType = DesiredType.GetElementType();
                int i = 0;

                switch (Type.GetTypeCode(ElementType))
                {
                    case TypeCode.Byte:
                        byte[] ba = new byte[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= byte.MinValue && d <= byte.MaxValue)
                                ba[i++] = (byte)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = ba;
                        return true;

                    case TypeCode.Decimal:
                        decimal[] da = new decimal[this.dimension];
                        foreach (double d in this.Values)
                            da[i++] = (decimal)d;

                        Value = da;
                        return true;

                    case TypeCode.Int16:
                        short[] sa = new short[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= short.MinValue && d <= short.MaxValue)
                                sa[i++] = (short)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = sa;
                        return true;

                    case TypeCode.Int32:
                        int[] ia = new int[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= int.MinValue && d <= int.MaxValue)
                                ia[i++] = (byte)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = ia;
                        return true;

                    case TypeCode.Int64:
                        long[] la = new long[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= long.MinValue && d <= long.MaxValue)
                                la[i++] = (long)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = la;
                        return true;

                    case TypeCode.SByte:
                        sbyte[] sba = new sbyte[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= sbyte.MinValue && d <= sbyte.MaxValue)
                                sba[i++] = (sbyte)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = sba;
                        return true;

                    case TypeCode.Single:
                        float[] fa = new float[this.dimension];
                        foreach (double d in this.Values)
                            fa[i++] = (float)d;

                        Value = fa;
                        return true;

                    case TypeCode.UInt16:
                        ushort[] usa = new ushort[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= ushort.MinValue && d <= ushort.MaxValue)
                                usa[i++] = (ushort)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = usa;
                        return true;

                    case TypeCode.UInt32:
                        uint[] uia = new uint[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= uint.MinValue && d <= uint.MaxValue)
                                uia[i++] = (uint)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = uia;
                        return true;

                    case TypeCode.UInt64:
                        ulong[] ula = new ulong[this.dimension];
                        foreach (double d in this.Values)
                        {
                            if (d >= ulong.MinValue && d <= ulong.MaxValue)
                                ula[i++] = (ulong)d;
                            else
                            {
                                Value = null;
                                return false;
                            }
                        }

                        Value = ula;
                        return true;

                    default:
                        Value = this;
                        return true;
                }
            }
            else
            {
                Value = null;
                return false;
            }
        }

    }
}
