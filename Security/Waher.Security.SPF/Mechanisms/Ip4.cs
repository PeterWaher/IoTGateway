using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Waher.Security.SPF.Mechanisms
{
	/// <summary>
	/// This mechanisms tests whether &lt;ip&gt; is contained within a given IP4 network.
	/// </summary>
	public class Ip4 : Ip
	{
		/// <summary>
		/// This mechanisms tests whether &lt;ip&gt; is contained within a given IP4 network.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Ip4(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
			if (this.address.AddressFamily != AddressFamily.InterNetwork)
				throw new Exception("Expected IP4 address.");
		}
	}
}
