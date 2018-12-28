using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Pseudo-vector space of Object-valued vectors.
	/// </summary>
	public sealed class ObjectVectors : VectorSpace
	{
		private readonly ObjectVector referenceVector;
		private readonly int dimension;

		/// <summary>
		/// Pseudo-vector space of Object-valued vectors.
		/// </summary>
		/// <param name="ReferenceVector">Reference vector.</param>
		public ObjectVectors(ObjectVector ReferenceVector)
		{
			this.dimension = ReferenceVector.Dimension;
			this.referenceVector = ReferenceVector;
		}

		/// <summary>
		/// Dimension of vector space.
		/// </summary>
		public int Dimension
		{
			get { return this.dimension; }
		}

		/// <summary>
		/// Scalar ring.
		/// </summary>
		public override IRing ScalarRing
		{
			get
			{
				if (!(this.scalarRing is null))
					return this.scalarRing;

				IElement Ref = null;
				IElement Ref2 = null;
				ISet Set = null;
				ISet Set2 = null;

				foreach (IElement Element in this.referenceVector.Elements)
				{
					if (Set is null)
					{
						Ref = Element;
						Set = Element.AssociatedSet;
					}
					else
					{
						Set2 = Element.AssociatedSet;
						if (!Set.Equals(Set2))
						{
							if (!Expression.UpgradeField(ref Ref, ref Set, ref Ref2, ref Set2, null))
								throw new ScriptException("No common scalar ring found.");
						}
					}
				}

				this.scalarRing = Set as IRing;
				if (this.scalarRing is null)
					throw new ScriptException("No common scalar ring found.");

				return this.scalarRing;
			}
		}

		private IRing scalarRing = null;

		/// <summary>
		/// Scalar field.
		/// </summary>
		public override IField ScalarField
		{
			get
			{
				if (!(this.scalarField is null))
					return this.scalarField;

				this.scalarField = this.ScalarRing as IField;
				if (this.scalarField is null)
					throw new ScriptException("No common scalar field found.");

				return this.scalarField;
			}
		}

		private IField scalarField = null;

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get
			{
				return this.ScalarRing.Zero;
			}
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			ObjectVector v = Element as ObjectVector;
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
			return (obj is ObjectVectors v && v.dimension == this.dimension);
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
