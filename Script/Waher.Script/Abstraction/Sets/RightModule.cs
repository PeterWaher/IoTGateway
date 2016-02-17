using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of right modules.
	/// </summary>
	public abstract class RightModule : AbelianGroup, IRightModule
	{
		/// <summary>
		/// Base class for all types of right modules.
		/// </summary>
		public RightModule()
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
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public abstract IRightModuleElement MultiplyScalarRight(IRightModuleElement ModuleElement, IRingElement Scalar);

	}
}
