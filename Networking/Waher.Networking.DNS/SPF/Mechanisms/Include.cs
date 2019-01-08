using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// The "include" mechanism triggers a recursive evaluation of check_host().
	/// </summary>
	public class Include : MechanismDomainSpec	
	{
		/// <summary>
		/// The "include" mechanism triggers a recursive evaluation of check_host().
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Include(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}
	}
}
