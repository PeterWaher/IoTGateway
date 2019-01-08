using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism tests whether the DNS reverse-mapping for &lt;ip&gt; exists
	/// and correctly points to a domain name within a particular domain.
	/// This mechanism SHOULD NOT be published.See the note at the end of
	/// this section for more information.
	/// </summary>
	public class Ptr : MechanismDomainSpec
	{
		/// <summary>
		/// This mechanism tests whether the DNS reverse-mapping for &lt;ip&gt; exists
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

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override async Task<SpfResult> Matches()
		{
			try
			{
				if (this.term.dnsLookupsLeft-- <= 0)
					throw new Exception("DNS Lookup maximum reached.");

				string TargetDomain = this.TargetDomain;
				string[] DomainNames = await DnsResolver.LookupDomainName(this.term.ip);

				// First check if domain is found.

				foreach (string DomainName in DomainNames)
				{
					if (string.Compare(DomainName, TargetDomain, true) == 0 &&
						await this.MatchReverseIp(DomainName))
					{
						return SpfResult.Pass;
					}
				}

				// Second, check if sub-domain is found.

				foreach (string DomainName in DomainNames)
				{
					if (DomainName.EndsWith("." + TargetDomain, StringComparison.CurrentCultureIgnoreCase) &&
						await this.MatchReverseIp(DomainName))
					{
						return SpfResult.Pass;
					}
				}
			}
			catch (Exception)
			{
				// Fail
			}

			return SpfResult.Fail;
		}
	}
}
