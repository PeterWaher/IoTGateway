using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of right module elements.
	/// </summary>
	public interface IRightModuleElement : IAbelianGroupElement
	{
		/// <summary>
		/// Tries to multiply a scalar to the current element from the right.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRightModuleElement MultiplyScalarRight(IRingElement Scalar);

		/// <summary>
		/// Associated Right-Module.
		/// </summary>
		IRightModule AssociatedRightModule
		{
			get;
		}
	}
}
