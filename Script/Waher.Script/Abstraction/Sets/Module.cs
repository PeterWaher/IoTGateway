using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of modules.
	/// </summary>
	public abstract class Module : AbelianGroup, IModule
	{
		/// <summary>
		/// Base class for all types of left modules.
		/// </summary>
		public Module()
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
		public virtual ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar, ILeftModuleElement ModuleElement)
		{
			if (ModuleElement is IModuleElement E)
				return this.MultiplyScalar(Scalar, E);
			else
				return null;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IRightModuleElement MultiplyScalarRight(IRightModuleElement ModuleElement, IRingElement Scalar)
		{
			if (ModuleElement is IModuleElement E)
				return this.MultiplyScalar(Scalar, E);
			else
				return null;
		}

		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual IModuleElement MultiplyScalar(IRingElement Scalar, IModuleElement ModuleElement)
		{
			return ModuleElement.MultiplyScalar(Scalar);
		}

	}
}
