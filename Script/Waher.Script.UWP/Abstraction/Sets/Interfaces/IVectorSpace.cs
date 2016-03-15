using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of modules.
	/// </summary>
	public interface IVectorSpace : IModule 
	{
		/// <summary>
		/// Scalar field.
		/// </summary>
		IField ScalarField
		{
			get;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="VectorSpaceElement">Vector Space element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IVectorSpaceElement MultiplyScalar(IFieldElement Scalar, IVectorSpaceElement VectorSpaceElement);
	}
}
