using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism matches if <ip> is one of the <target-name>'s IP
	/// addresses.For clarity, this means the "a" mechanism also matches
	/// AAAA records.
	/// </summary>
	public class A : MechanismDomainCidrSpec
	{
		/// <summary>
		/// This mechanism matches if <ip> is one of the <target-name>'s IP
		/// addresses.For clarity, this means the "a" mechanism also matches
		/// AAAA records.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public A(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}
	}
}
