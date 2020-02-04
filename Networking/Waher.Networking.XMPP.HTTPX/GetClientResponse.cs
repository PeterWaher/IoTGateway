using System;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Response to the <see cref="HttpxProxy.GetClientAsync(Uri)"/> method call.
	/// </summary>
	public class GetClientResponse
	{
		/// <summary>
		/// Full JID of entity hosting the resource.
		/// </summary>
		public string FullJid;

		/// <summary>
		/// Bare JID of entity hosting the resource.
		/// </summary>
		public string BareJid;

		/// <summary>
		/// Local part of the URL
		/// </summary>
		public string LocalUrl;

		/// <summary>
		/// Corresponding <see cref="HttpxClient"/> object to use for the request..
		/// </summary>
		public HttpxClient HttpxClient;
	}
}
