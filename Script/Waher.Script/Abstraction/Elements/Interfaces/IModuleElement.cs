using System;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of module elements.
	/// </summary>
	public interface IModuleElement : ILeftModuleElement, IRightModuleElement
	{
		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		IModuleElement MultiplyScalar(IRingElement Scalar);

		/// <summary>
		/// Associated Right-Module.
		/// </summary>
		Sets.IModule AssociatedModule
		{
			get;
		}
	}
}
