using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// Abstract base class for SPF Mechanisms.
	/// </summary>
	public abstract class Mechanism
	{
		/// <summary>
		/// Qualifier
		/// </summary>
		protected SpfQualifier qualifier;

		/// <summary>
		/// Abstract base class for SPF Mechanisms.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Mechanism(Term Term, SpfQualifier Qualifier)
		{
			this.qualifier = Qualifier;
		}
	}
}
