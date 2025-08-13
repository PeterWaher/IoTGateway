﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Objects.VectorSpaces
{
    /// <summary>
    /// DateTime-valued vector.
    /// </summary>
    public sealed class DateTimeVector : VectorSpaceElement
    {
        private DateTime[] values;
        private ICollection<IElement> elements;
        private readonly int dimension;

        /// <summary>
        /// DateTime-valued vector.
        /// </summary>
        /// <param name="Values">DateTime values.</param>
        public DateTimeVector(params DateTime[] Values)
        {
            this.values = Values;
            this.elements = null;
            this.dimension = Values.Length;
        }

        /// <summary>
        /// DateTime-valued vector.
        /// </summary>
        /// <param name="Elements">Elements.</param>
        public DateTimeVector(ICollection<IElement> Elements)
        {
            this.values = null;
            this.elements = Elements;
            this.dimension = Elements.Count;
        }

        /// <summary>
        /// Vector element values.
        /// </summary>
        public DateTime[] Values
        {
            get
            {
                if (this.values is null)
                {
					DateTime[] v = new DateTime[this.dimension];
                    int i = 0;

                    if (this.elements is ChunkedList<IElement> Values)
                    {
						ChunkNode<IElement> Loop = Values.FirstChunk;
						int j, c;

						while (!(Loop is null))
						{
							for (j = Loop.Start, c = Loop.Pos; j < c; j++)
							{
                                if (!(Loop[j].AssociatedObjectValue is DateTime TP))
                                    TP = DateTime.MinValue;

								v[i++] = TP;
							}

							Loop = Loop.Next;
						}
					}
					else
                    {
                        foreach (IElement Element in this.elements)
                        {
                            if (!(Element.AssociatedObjectValue is DateTime TP))
                                TP = DateTime.MinValue;

                            v[i++] = TP;
                        }
                    }

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
                if (this.elements is null)
                {
                    int i;
                    IElement[] v = new IElement[this.dimension];

                    for (i = 0; i < this.dimension; i++)
                        v[i] = new DateTimeValue(this.values[i]);

                    this.elements = v;
                }

                return this.elements;
            }
        }

        /// <summary>
        /// Dimension of vector.
        /// </summary>
        public override int Dimension => this.dimension;

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = null;

            foreach (DateTime d in this.Values)
            {
                if (sb is null)
                    sb = new StringBuilder("[");
                else
                    sb.Append(", ");

                sb.Append(Expression.ToString(d));
            }

            if (sb is null)
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
                if (this.associatedVectorSpace is null)
                    this.associatedVectorSpace = new DateTimeVectors(this.dimension);

                return this.associatedVectorSpace;
            }
        }

        private DateTimeVectors associatedVectorSpace = null;

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue => this.Values;

        /// <summary>
        /// Tries to multiply a scalar to the current element.
        /// </summary>
        /// <param name="Scalar">Scalar to multiply.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override IVectorSpaceElement MultiplyScalar(IFieldElement Scalar)
        {
            return null;
        }

        /// <summary>
        /// Tries to add an element to the current element.
        /// </summary>
        /// <param name="Element">Element to add.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override IAbelianGroupElement Add(IAbelianGroupElement Element)
        {
            return null;
        }

        /// <summary>
        /// Negates the element.
        /// </summary>
        /// <returns>Negation of current element.</returns>
        public override IGroupElement Negate()
        {
            return null;
        }

        /// <summary>
        /// Compares the element to another.
        /// </summary>
        /// <param name="obj">Other element to compare against.</param>
        /// <returns>If elements are equal.</returns>
        public override bool Equals(object obj)
        {
            DateTimeVector DateTimeVector = obj as DateTimeVector;
            if (DateTimeVector is null)
                return false;

            int i;
            if (DateTimeVector.dimension != this.dimension)
                return false;

			DateTime[] Values = this.Values;
			DateTime[] Values2 = DateTimeVector.Values;
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
			DateTime[] Values = this.Values;
            int Result = 0;
            int i;

            for (i = 0; i < this.dimension; i++)
                Result ^= Values[i].GetHashCode();

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
        public override ICollection<IElement> ChildElements => this.Elements;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public override IElement Encapsulate(ChunkedList<IElement> Elements, ScriptNode Node)
		{
			return VectorDefinition.Encapsulate(Elements, true, Node);
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
                if (this.zero is null)
                    this.zero = new DateTimeVector(new DateTime[this.dimension]);

                return this.zero;
            }
        }

        private DateTimeVector zero = null;

        /// <summary>
        /// Gets an element of the vector.
        /// </summary>
        /// <param name="Index">Zero-based index into the vector.</param>
        /// <returns>Vector element.</returns>
        public override IElement GetElement(int Index)
        {
            if (Index < 0 || Index >= this.dimension)
                throw new ScriptException("Index out of bounds.");

			DateTime[] V = this.Values;

            return new DateTimeValue(V[Index]);
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

            if (!(Value.AssociatedObjectValue is DateTime V))
                throw new ScriptException("Elements in a boolean vector are required to be boolean values.");

            DateTime[] Values = this.Values;
            this.elements = null;

            Values[Index] = V;
        }

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
            if (DesiredType == typeof(DateTime[]))
            {
                Value = this.Values;
                return true;
            }
            else if (DesiredType.IsAssignableFrom(typeof(DateTimeVector).GetTypeInfo()))
            {
                Value = this;
                return true;
            }
            else
				return Expression.TryConvert(this.Values, DesiredType, out Value);
		}

	}
}