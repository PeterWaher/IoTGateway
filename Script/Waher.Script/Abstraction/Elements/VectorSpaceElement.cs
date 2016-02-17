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
			IFieldElement FieldElement = Scalar as IFieldElement;
			if (FieldElement == null)
				return null;
			else
				return this.MultiplyScalar(FieldElement);
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element from the right.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IRightModuleElement MultiplyScalarRight(IRingElement Scalar)
		{
			IFieldElement FieldElement = Scalar as IFieldElement;
			if (FieldElement == null)
				return null;
			else
				return this.MultiplyScalar(FieldElement);
		}

		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IModuleElement MultiplyScalar(IRingElement Scalar)
		{
			IFieldElement FieldElement = Scalar as IFieldElement;
			if (FieldElement == null)
				return null;
			else
				return this.MultiplyScalar(FieldElement);
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
	}
}
