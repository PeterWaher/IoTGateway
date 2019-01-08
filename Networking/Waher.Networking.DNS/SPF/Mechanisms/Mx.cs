using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism matches if <ip> is one of the MX hosts for a domain name.
	/// </summary>
	internal class Mx : MechanismDomainCidrSpec
	{
		/// <summary>
		/// This mechanism matches if <ip> is one of the MX hosts for a domain name.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		internal Mx(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}
	}
}
