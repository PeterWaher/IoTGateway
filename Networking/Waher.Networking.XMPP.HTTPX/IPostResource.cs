using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Interface for HTTP(S) Post-back resources. These can be used to allow HTTPX servers to HTTP POST back responses
	/// of large content, when other P2P options are not available.
	/// </summary>
	public interface IPostResource
	{
		/// <summary>
		/// Gets a Post-back URL
		/// </summary>
		/// <param name="Callback">Method to call when item has been posted to URL.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>URL the HTTPX server can use to post back responses to.</returns>
		string GetUrl(PostBackEventHandler Callback, object State);
	}
}
