using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of left modules.
	/// </summary>
	public abstract class LeftModule : AbelianGroup, ILeftModule
	{
		/// <summary>
		/// Base class for all types of left modules.
		/// </summary>
		public LeftModule()
			: base()
		{
		}

		/// <summary>
		/// Scalar ring.
		/// </summary>
		public abstract IRing ScalarRing
		{
			get;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="Scalar">Scalar element.</param>
		/// <param name="ModuleElement">Module element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar, ILeftModuleElement ModuleElement);

	}
}
