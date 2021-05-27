using System;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of integral domains.
	/// </summary>
	public abstract class IntegralDomain : CommutativeRingWithIdentity, IIntegralDomain
	{
		/// <summary>
		/// Base class for all types of integral domains.
		/// </summary>
		public IntegralDomain()
			: base()
		{
		}

	}
}
