using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of integral domain elements.
	/// </summary>
	public interface IIntegralDomainElement : ICommutativeRingWithIdentityElement
	{
		/// <summary>
		/// Associated Integral Domain.
		/// </summary>
		IIntegralDomain AssociatedIntegralDomain
		{
			get;
		}
	}
}
