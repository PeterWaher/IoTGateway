using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Waher.Security.SPF.Mechanisms
{
	/// <summary>
	/// Abstract base class for SPF mechanisms with a domain specification and
	/// an optional CIDR specification.
	/// </summary>
	public abstract class MechanismDomainCidrSpec : MechanismDomainSpec
	{
		/// <summary>
		/// IPv4 CIDR
		/// </summary>
		protected readonly int ip4Cidr = 32;

		/// <summary>
		/// IPv6 CIDR
		/// </summary>
		protected readonly int ip6Cidr = 128;

		/// <summary>
		/// Abstract base class for SPF mechanisms with a domain specification and
		/// an optional CIDR specification.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public MechanismDomainCidrSpec(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
			if (Term.PeekNextChar() == '/')
			{
				Term.NextChar();

				bool HasIp4;

				if (HasIp4 = char.IsDigit(Term.PeekNextChar()))
				{
					this.ip4Cidr = Term.NextInteger();
					if (this.ip4Cidr < 0 || this.ip4Cidr > 32)
						throw new Exception("Invalid IP4 CIDR");
				}

				if (Term.PeekNextChar() == '/')
				{
					Term.NextChar();

					if (HasIp4 && Term.PeekNextChar() == '/')
						Term.NextChar();

					if (char.IsDigit(Term.PeekNextChar()))
					{
						this.ip6Cidr = Term.NextInteger();
						if (this.ip6Cidr < 0 || this.ip4Cidr > 128)
							throw new Exception("Invalid IP6 CIDR");
					}
					else if (!HasIp4)
						throw new Exception("IP4 or IP6 CIDR expected.");
				}
			}
		}

		/// <summary>
		/// If the domain specification is required.
		/// </summary>
		public override bool DomainRequired => false;

		/// <summary>
		/// Checks if the client IP address matches any of a given set of IP Addresses,
		/// taking CIDR into account.
		/// </summary>
		/// <param name="Addresses">Addresses to check</param>
		/// <param name="Term">Current query</param>
		/// <param name="Cidr">CIDR</param>
		/// <returns>Match result</returns>
		internal static bool Matches(IPAddress[] Addresses, Term Term, int Cidr)
		{
			byte[] Bin1 = Term.ip.GetAddressBytes();
			int c = Bin1.Length;

			foreach (IPAddress Addr in Addresses)
			{
				byte[] Bin2 = Addr.GetAddressBytes();
				if (Bin2.Length != c)
					continue;

				int BitsLeft = Cidr;
				int Pos = 0;

				while (BitsLeft > 0 && Pos < c)
				{
					if (BitsLeft >= 8)
					{
						if (Bin1[Pos] != Bin2[Pos])
							break;

						BitsLeft -= 8;
					}
					else
					{
						byte Mask = (byte)(0xff << (8 - BitsLeft));

						if ((Bin1[Pos] & Mask) != (Bin2[Pos] & Mask))
							break;

						BitsLeft = 0;
					}

					Pos++;
				}

				if (BitsLeft == 0)
					return true;
			}

			return false;
		}
	}
}
