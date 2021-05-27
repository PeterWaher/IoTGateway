using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of right modules.
	/// </summary>
	public interface IRightModule : IAbelianGroup
	{
		/// <summary>
		/// Scalar ring.
		/// </summary>
		IRing ScalarRing
		{
			get;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IRightModuleElement MultiplyScalarRight(IRightModuleElement ModuleElement, IRingElement Scalar);

	}
}
