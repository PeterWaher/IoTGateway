using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Pseudo-vector space of Complex-valued vectors.
	/// </summary>
	public sealed class ComplexVectors : VectorSpace
	{
		private static readonly ComplexNumbers scalarField = new ComplexNumbers();

		private ComplexVector zero = null;
		private int dimension;

		/// <summary>
		/// Pseudo-vector space of Complex-valued vectors.
		/// </summary>
		/// <param name="Dimension">Dimension.</param>
		public ComplexVectors(int Dimension)
		{
			this.dimension = Dimension;
		}

		/// <summary>
		/// Dimension of vector space.
		/// </summary>
		public int Dimension
		{
			get { return this.dimension; }
		}

		/// <summary>
		/// Scalar field.
		/// </summary>
		public override IField ScalarField
		{
			get { return scalarField; }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get 
			{
                if (this.zero is null)
                {
                    Complex[] v = new Complex[this.dimension];
                    int i;

                    for (i = 0; i < this.dimension; i++)
                        v[i] = Complex.Zero;

                    this.zero = new ComplexVector(v);
                }

				return this.zero;
			}
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			ComplexVector v = Element as ComplexVector;
			if (v is null)
				return false;

			return v.Dimension == this.dimension;
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			ComplexVectors v = obj as ComplexVectors;
			return (v != null && v.dimension == this.dimension);
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			return this.dimension.GetHashCode();
		}

	}
}
