using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of vector spaces.
	/// </summary>
	public abstract class VectorSpace : Module, IVectorSpace
	{
		/// <summary>
		/// Base class for all types of vector spaces.
		/// </summary>
		public VectorSpace()
			: base()
		{
		}

		/// <summary>
		/// Scalar ring.
		/// </summary>
		public override IRing ScalarRing
		{
			get { return this.ScalarField; }
		}

		/// <summary>
		/// Scalar field.
		/// </summary>
		public abstract IField ScalarField
		{
			get;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IModuleElement MultiplyScalar(IRingElement Scalar, IModuleElement ModuleElement)
		{
			return ModuleElement.MultiplyScalar(Scalar);
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="VectorSpaceElement">Vector Space element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IVectorSpaceElement MultiplyScalar(IFieldElement Scalar, IVectorSpaceElement VectorSpaceElement)
		{
			return VectorSpaceElement.MultiplyScalar(Scalar);
		}

	}
}
