using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// This mechanism matches if &lt;ip&gt; is one of the &lt;target-name&gt;'s IP
	/// addresses.For clarity, this means the "a" mechanism also matches
	/// AAAA records.
	/// </summary>
	public class A : MechanismDomainCidrSpec
	{
		/// <summary>
		/// This mechanism matches if &lt;ip&gt; is one of the &lt;target-name&gt;'s IP
		/// addresses.For clarity, this means the "a" mechanism also matches
		/// AAAA records.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public A(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override async Task<SpfResult> Matches()
		{
			return await Matches(this.TargetDomain, this.term, this.ip4Cidr, this.ip6Cidr) ? SpfResult.Pass : SpfResult.Fail;
		}

		internal static async Task<bool> Matches(string Domain, Term Term, int Cidr4, int Cidr6)
		{
			IPAddress[] Addresses;
			int Cidr;

			switch (Term.ip.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					if (Term.dnsLookupsLeft-- <= 0)
						throw new Exception("DNS Lookup maximum reached.");

					Addresses = await DnsResolver.LookupIP4Addresses(Domain);
					Cidr = Cidr4;
					break;

				case AddressFamily.InterNetworkV6:
					if (Term.dnsLookupsLeft-- <= 0)
						throw new Exception("DNS Lookup maximum reached.");

					Addresses = await DnsResolver.LookupIP6Addresses(Domain);
					Cidr = Cidr6;
					break;

				default:
					return false;
			}

			return Matches(Addresses, Term, Cidr);
		}
	}
}
