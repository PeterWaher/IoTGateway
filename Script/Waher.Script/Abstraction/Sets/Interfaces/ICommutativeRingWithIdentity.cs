using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for all types of commutative rings with identity.
	/// </summary>
	public interface ICommutativeRingWithIdentity : ICommutativeRing
	{
		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		ICommutativeRingWithIdentityElement One
		{
			get;
		}
	}
}
