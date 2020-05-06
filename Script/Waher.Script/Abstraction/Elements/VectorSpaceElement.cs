using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Base class for all types of vector space elements (vectors).
	/// </summary>
	public abstract class VectorSpaceElement : ModuleElement, IVectorSpaceElement
	{
		/// <summary>
		/// Base class for all types of vector space elements (vectors).
		/// </summary>
		public VectorSpaceElement()
		{
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the left.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar)
		{
			if (Scalar is IFieldElement FieldElement)
				return this.MultiplyScalar(FieldElement);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the right.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRightModuleElement MultiplyScalarRight(IRingElement Scalar)
		{
			if (Scalar is IFieldElement FieldElement)
				return this.MultiplyScalar(FieldElement);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IModuleElement MultiplyScalar(IRingElement Scalar)
		{
			if (Scalar is IFieldElement FieldElement)
				return this.MultiplyScalar(FieldElement);
			else
				return null;
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract IVectorSpaceElement MultiplyScalar(IFieldElement Scalar);

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Group.
		/// </summary>
		public override IGroup AssociatedGroup
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Abelian Group.
		/// </summary>
		public override IAbelianGroup AssociatedAbelianGroup
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Left-Module.
		/// </summary>
		public override ILeftModule AssociatedLeftModule
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Right-Module.
		/// </summary>
		public override IRightModule AssociatedRightModule
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Module.
		/// </summary>
		public override Sets.IModule AssociatedModule
		{
			get { return this.AssociatedVectorSpace; }
		}

		/// <summary>
		/// Associated Right-VectorSpace.
		/// </summary>
		public abstract IVectorSpace AssociatedVectorSpace
		{
			get;
		}

		/// <summary>
		/// Dimension of vector.
		/// </summary>
		public abstract int Dimension
		{
			get;
		}

		/// <summary>
		/// An enumeration of vector elements.
		/// </summary>
		public virtual ICollection<IElement> VectorElements
		{
			get { return this.ChildElements; }
		}

        /// <summary>
        /// Gets an element of the vector.
        /// </summary>
        /// <param name="Index">Zero-based index into the vector.</param>
        /// <returns>Vector element.</returns>
        public abstract IElement GetElement(int Index);

        /// <summary>
        /// Sets an element in the vector.
        /// </summary>
        /// <param name="Index">Index.</param>
        /// <param name="Value">Element to set.</param>
        public abstract void SetElement(int Index, IElement Value);
    }
}
