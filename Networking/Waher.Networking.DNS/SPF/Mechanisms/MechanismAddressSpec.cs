using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// Abstract base class for SPF mechanisms with a domain specification and
	/// an optional CIDR specification.
	/// </summary>
	public abstract class MechanismDomainCidrSpec : MechanismDomainSpec
	{
		private readonly int ip4Cidr;
		private readonly int ip6Cidr;

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
				else
					this.ip4Cidr = 32;

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
					else
						this.ip6Cidr = 128;
				}
			}
		}

		public override bool DomainRequired => false;
	}
}
