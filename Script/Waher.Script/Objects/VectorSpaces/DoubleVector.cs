using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
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
		/// <param name="Values">Double value.</param>
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

				sb.Append(d.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
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
			return VectorDefinition.Encapsulate(Elements, Node);
		}

	}
}
