using System;
using Waher.Script.Abstraction.Sets;

namespace Waher.Script.Abstraction.Elements
{
	/// <summary>
	/// Basic interface for all types of field elements.
	/// </summary>
	public interface IFieldElement : IEuclidianDomainElement 
	{
		/// <summary>
		/// Associated Field.
		/// </summary>
		IField AssociatedField
		{
			get;
		}
	}
}
