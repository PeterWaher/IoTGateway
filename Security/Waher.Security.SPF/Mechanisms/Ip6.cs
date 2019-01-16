using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Waher.Security.SPF.Mechanisms
{
	/// <summary>
	/// This mechanisms tests whether &lt;ip&gt; is contained within a given IP6 network.
	/// </summary>
	public class Ip6 : Ip
	{
		/// <summary>
		/// This mechanisms tests whether &lt;ip&gt; is contained within a given IP6 network.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Ip6(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
			if (this.address.AddressFamily != AddressFamily.InterNetworkV6)
				throw new Exception("Expected IP6 address.");
		}
	}
}
