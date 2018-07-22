using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Pseudo-vector space of Double-valued vectors.
	/// </summary>
	public sealed class DoubleVectors : VectorSpace
	{
		private DoubleVector zero = null;
		private int dimension;

		/// <summary>
		/// Pseudo-vector space of Double-valued vectors.
		/// </summary>
		/// <param name="Dimension">Dimension.</param>
		public DoubleVectors(int Dimension)
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
			get { return DoubleNumbers.Instance; }
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

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			if (!(Element is DoubleVector v))
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
			return (obj is DoubleVectors v && v.dimension == this.dimension);
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
