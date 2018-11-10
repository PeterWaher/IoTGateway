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
		private const int Code = 511;
		private const string Msg = "Network Authentication Required";

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		public NetworkAuthenticationRequiredException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NetworkAuthenticationRequiredException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The client needs to authenticate to gain network access. Intended for use by intercepting proxies used to control access to the network.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NetworkAuthenticationRequiredException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
