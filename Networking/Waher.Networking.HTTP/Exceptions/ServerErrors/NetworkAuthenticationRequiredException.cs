using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
	/// </summary>
	public class NetworkAuthenticationRequiredException : HttpException
	{
		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		public NetworkAuthenticationRequiredException()
			: base(511, "Network Authentication Required")
		{
		}
	}
}
