using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of Euclidian domain elements.
	/// </summary>
	public interface IEuclidianDomainElement : IIntegralDomainElement 
	{
		/// <summary>
		/// Associated Euclidian Domain.
		/// </summary>
		IEuclidianDomain AssociatedEuclidianDomain
		{
			get;
		}
	}
}
