using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of modules.
	/// </summary>
	public interface IModule : ILeftModule, IRightModule 
	{
		/// <summary>
		/// Performs a scalar multiplication, if possible.
		/// </summary>
		/// <param name="ModuleElement">Module element.</param>
		/// <param name="Scalar">Scalar element.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IModuleElement MultiplyScalar(IRingElement Scalar, IModuleElement ModuleElement);
	}
}
