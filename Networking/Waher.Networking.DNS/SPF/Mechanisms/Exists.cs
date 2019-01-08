using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism is used to construct an arbitrary domain name that is
	/// used for a DNS A record query.It allows for complicated schemes
	/// involving arbitrary parts of the mail envelope to determine what is
	/// permitted.
	/// </summary>
	public class Exists : MechanismDomainSpec
	{
		/// <summary>
		/// This mechanism is used to construct an arbitrary domain name that is
		/// used for a DNS A record query.It allows for complicated schemes
		/// involving arbitrary parts of the mail envelope to determine what is
		/// permitted.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Exists(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}
	}
}
