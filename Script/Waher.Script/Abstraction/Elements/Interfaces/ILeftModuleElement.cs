using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of left module elements.
	/// </summary>
	public interface ILeftModuleElement : IAbelianGroupElement
	{
		/// <summary>
		/// Tries to multiply a scalar to the current element from the left.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		ILeftModuleElement MultiplyScalarLeft(IRingElement Scalar);

		/// <summary>
		/// Associated Left-Module.
		/// </summary>
		ILeftModule AssociatedLeftModule
		{
			get;
		}
	}
}
