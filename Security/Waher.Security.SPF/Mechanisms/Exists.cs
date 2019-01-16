using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.DNS;

namespace Waher.Security.SPF.Mechanisms
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

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override async Task<SpfResult> Matches()
		{
			if (this.term.dnsLookupsLeft-- <= 0)
				throw new Exception("DNS Lookup maximum reached.");

			try
			{
				IPAddress[] Addresses = await DnsResolver.LookupIP4Addresses(Domain);   // Always IPv4, regardless of connection type.
				if (Addresses is null || Addresses.Length == 0)
					return SpfResult.Fail;
				else
					return SpfResult.Pass;
			}
			catch (Exception)
			{
				return SpfResult.Fail;
			}
		}
	}
}
