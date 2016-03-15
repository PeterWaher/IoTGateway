using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of commutative ring with identity elements.
	/// </summary>
	public interface ICommutativeRingWithIdentityElement : ICommutativeRingElement 
	{
		/// <summary>
		/// Associated Commutative Ring With Identity.
		/// </summary>
		ICommutativeRingWithIdentity AssociatedCommutativeRingWithIdentity
		{
			get;
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		ICommutativeRingWithIdentityElement One
		{
			get;
		}
	}
}
