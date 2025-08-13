﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Complex-valued vector.
	/// </summary>
	public sealed class ComplexVector : VectorSpaceElement
	{
		private Complex[] values;
		private ICollection<IElement> elements;
		private readonly int dimension;

		/// <summary>
		/// Complex-valued vector.
		/// </summary>
		/// <param name="Values">Complex values.</param>
		public ComplexVector(params Complex[] Values)
		{
			this.values = Values;
			this.elements = null;
			this.dimension = Values.Length;
		}

		/// <summary>
		/// Complex-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ComplexVector(ICollection<IElement> Elements)
		{
			this.values = null;
			this.elements = Elements;
			this.dimension = Elements.Count;
		}

		/// <summary>
		/// Vector element values.
		/// </summary>
		public Complex[] Values
		{
			get
			{
				if (this.values is null)
				{
					Complex[] v = new Complex[this.dimension];
					int i = 0;

					if (this.elements is ChunkedList<IElement> Values)
					{
						ChunkNode<IElement> Loop = Values.FirstChunk;
						int j, c;

						while (!(Loop is null))
						{
							for (j = Loop.Start, c = Loop.Pos; j < c; j++)
							{
								if (!(Loop[j].AssociatedObjectValue is Complex z))
									z = 0;

								v[i++] = z;
							}

							Loop = Loop.Next;
						}
					}
					else
					{
						foreach (IElement Element in this.elements)
						{
							if (!(Element.AssociatedObjectValue is Complex z))
								z = 0;

							v[i++] = z;
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
						v[i] = new ComplexNumber(this.values[i]);

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
			return Expression.ToString(this.Values);
		}

		/// <summary>
		/// Associated Right-VectorSpace.
		/// </summary>
		public override IVectorSpace AssociatedVectorSpace
		{
			get
			{
				if (this.associatedVectorSpace is null)
					this.associatedVectorSpace = new ComplexVectors(this.dimension);

				return this.associatedVectorSpace;
			}
		}

		private ComplexVectors associatedVectorSpace = null;

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
			ComplexNumber ComplexNumber = Scalar as ComplexNumber;
			Complex d;
			if (ComplexNumber is null)
			{
				DoubleNumber DoubleNumber = Scalar as DoubleNumber;
				if (DoubleNumber is null)
					return null;
				else
					d = new Complex(DoubleNumber.Value, 0);
			}
			else
				d = ComplexNumber.Value;

			int i;
			Complex[] Values = this.Values;
			Complex[] v = new Complex[this.dimension];

			for (i = 0; i < this.dimension; i++)
				v[i] = d * Values[i];

			return new ComplexVector(v);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			ComplexVector ComplexVector = Element as ComplexVector;
			if (ComplexVector is null)
				return null;

			int i;
			if (ComplexVector.dimension != this.dimension)
				return null;

			Complex[] Values = this.Values;
			Complex[] Values2 = ComplexVector.Values;
			Complex[] v = new Complex[this.dimension];
			for (i = 0; i < this.dimension; i++)
				v[i] = Values[i] + Values2[i];

			return new ComplexVector(v);
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			int i;
			Complex[] v = new Complex[this.dimension];
			Complex[] Values = this.Values;
			for (i = 0; i < this.dimension; i++)
				v[i] = -Values[i];

			return new ComplexVector(v);
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			ComplexVector ComplexVector = obj as ComplexVector;
			if (ComplexVector is null)
				return false;

			int i;
			if (ComplexVector.dimension != this.dimension)
				return false;

			Complex[] Values = this.Values;
			Complex[] Values2 = ComplexVector.Values;
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
			Complex[] Values = this.Values;
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
					this.zero = new ComplexVector(new Complex[this.dimension]);

				return this.zero;
			}
		}

		private ComplexVector zero = null;

		/// <summary>
		/// Gets an element of the vector.
		/// </summary>
		/// <param name="Index">Zero-based index into the vector.</param>
		/// <returns>Vector element.</returns>
		public override IElement GetElement(int Index)
		{
			if (Index < 0 || Index >= this.dimension)
				throw new ScriptException("Index out of bounds.");

			Complex[] V = this.Values;

			return new ComplexNumber(V[Index]);
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

			if (!(Value.AssociatedObjectValue is Complex V))
				throw new ScriptException("Elements in a complex vector are required to be complex values.");

			Complex[] Values = this.Values;
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
			if (DesiredType == typeof(Complex[]))
			{
				Value = this.Values;
				return true;
			}
			else if (DesiredType.IsAssignableFrom(typeof(ComplexVector).GetTypeInfo()))
			{
				Value = this;
				return true;
			}
			else if (DesiredType.IsArray)
			{
				Type ElementType = DesiredType.GetElementType();
				int i = 0;

				double d;

				if (ElementType == typeof(byte))
				{
					byte[] ba = new byte[this.dimension];

					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= byte.MinValue && d <= byte.MaxValue)
							ba[i++] = (byte)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = ba;
					return true;
				}
				else if (ElementType == typeof(decimal))
				{
					decimal[] da = new decimal[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0)
							da[i++] = (decimal)c.Real;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = da;
					return true;
				}
				else if (ElementType == typeof(double))
				{
					double[] da2 = new double[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0)
							da2[i++] = (double)c.Real;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = da2;
					return true;
				}
				else if (ElementType == typeof(short))
				{
					short[] sa = new short[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= short.MinValue && d <= short.MaxValue)
							sa[i++] = (short)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = sa;
					return true;
				}
				else if (ElementType == typeof(int))
				{
					int[] ia = new int[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= int.MinValue && d <= int.MaxValue)
							ia[i++] = (byte)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = ia;
					return true;
				}
				else if (ElementType == typeof(long))
				{
					long[] la = new long[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= long.MinValue && d <= long.MaxValue)
							la[i++] = (long)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = la;
					return true;
				}
				else if (ElementType == typeof(sbyte))
				{
					sbyte[] sba = new sbyte[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= sbyte.MinValue && d <= sbyte.MaxValue)
							sba[i++] = (sbyte)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = sba;
					return true;
				}
				else if (ElementType == typeof(float))
				{
					float[] fa = new float[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0)
							fa[i++] = (float)c.Real;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = fa;
					return true;
				}
				else if (ElementType == typeof(ushort))
				{
					ushort[] usa = new ushort[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= ushort.MinValue && d <= ushort.MaxValue)
							usa[i++] = (ushort)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = usa;
					return true;
				}
				else if (ElementType == typeof(uint))
				{
					uint[] uia = new uint[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= uint.MinValue && d <= uint.MaxValue)
							uia[i++] = (uint)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = uia;
					return true;
				}
				else if (ElementType == typeof(ulong))
				{
					ulong[] ula = new ulong[this.dimension];
					foreach (Complex c in this.Values)
					{
						if (c.Imaginary == 0 && (d = c.Real) >= ulong.MinValue && d <= ulong.MaxValue)
							ula[i++] = (ulong)d;
						else
						{
							Value = null;
							return false;
						}
					}

					Value = ula;
					return true;
				}
				else
				{
					Value = this;
					return true;
				}
			}
			else
				return Expression.TryConvert(this.Values, DesiredType, out Value);
		}

	}
}
