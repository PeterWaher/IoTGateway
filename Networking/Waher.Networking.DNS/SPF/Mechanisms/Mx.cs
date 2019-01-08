using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism matches if &lt;ip&gt; is one of the MX hosts for a domain name.
	/// </summary>
	internal class Mx : MechanismDomainCidrSpec
	{
		/// <summary>
		/// This mechanism matches if &lt;ip&gt; is one of the MX hosts for a domain name.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		internal Mx(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override async Task<SpfResult> Matches()
		{
			if (this.term.dnsLookupsLeft-- <= 0)
				throw new Exception("DNS Lookup maximum reached.");

			string TargetDomain = this.TargetDomain;
			string[] Exchanges = await DnsResolver.LookupMailExchange(TargetDomain);

			foreach (string Exchange in Exchanges)
			{
				IPAddress[] Addresses;
				int Cidr;
			
				switch (this.term.ip.AddressFamily)
				{
					case AddressFamily.InterNetwork:
						if (this.term.dnsLookupsLeft-- <= 0)
							throw new Exception("DNS Lookup maximum reached.");

						Addresses = await DnsResolver.LookupIP4Addresses(Exchange);
						Cidr = this.ip4Cidr;
						break;

					case AddressFamily.InterNetworkV6:
						if (this.term.dnsLookupsLeft-- <= 0)
							throw new Exception("DNS Lookup maximum reached.");

						Addresses = await DnsResolver.LookupIP6Addresses(Exchange);
						Cidr = this.ip6Cidr;
						break;

					default:
						return SpfResult.Fail;
				}

				if (Matches(Addresses, this.term, Cidr))
					return SpfResult.Pass;
			}

			return SpfResult.Fail;
		}
	}
}
