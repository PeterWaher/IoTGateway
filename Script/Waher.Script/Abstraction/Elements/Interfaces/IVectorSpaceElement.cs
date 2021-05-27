using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of module elements.
	/// </summary>
	public interface IVectorSpaceElement : IModuleElement, IVector
	{
		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IVectorSpaceElement MultiplyScalar(IFieldElement Scalar);

		/// <summary>
		/// Associated Right-VectorSpace.
		/// </summary>
		IVectorSpace AssociatedVectorSpace
		{
			get;
		}
	}
}
