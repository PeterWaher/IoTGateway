using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism tests whether the DNS reverse-mapping for <ip> exists
	/// and correctly points to a domain name within a particular domain.
	/// This mechanism SHOULD NOT be published.See the note at the end of
	/// this section for more information.
	/// </summary>
	public class Ptr : MechanismDomainSpec
	{
		/// <summary>
		/// This mechanism tests whether the DNS reverse-mapping for <ip> exists
		/// and correctly points to a domain name within a particular domain.
		/// This mechanism SHOULD NOT be published.See the note at the end of
		/// this section for more information.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Ptr(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// If the domain specification is required.
		/// </summary>
		public override bool DomainRequired => false;
	}
}
