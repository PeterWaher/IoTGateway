using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of left modules.
	/// </summary>
	public interface ILeftModule : IAbelianGroup
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
		/// <param name="Scalar">Scalar element.</param>
		/// <param name="ModuleElement">Module element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar, ILeftModuleElement ModuleElement);

	}
}
