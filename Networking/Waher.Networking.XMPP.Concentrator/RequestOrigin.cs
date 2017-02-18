using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Tokens available in request.
	/// </summary>
	public class RequestOrigin
	{
		private string[] deviceTokens;
		private string[] serviceTokens;
		private string[] userTokens;
		private string bareJid;

		/// <summary>
		/// Tokens available in request.
		/// </summary>
		/// <param name="BareJid">Bare JID of sender of request.</param>
		/// <param name="DeviceTokens">Device tokens, or null.</param>
		/// <param name="ServiceTokens">Service tokens, or null.</param>
		/// <param name="UserTokens">User tokens, or null.</param>
		public RequestOrigin(string BareJid, string[] DeviceTokens, string[] ServiceTokens, string[] UserTokens)
		{
			this.bareJid = BareJid;
			this.deviceTokens = DeviceTokens;
			this.serviceTokens = ServiceTokens;
			this.userTokens = UserTokens;
		}

		/// <summary>
		/// Bare JID of sender of request.
		/// </summary>
		public string BareJID
		{
			get { return this.bareJid; }
		}

		/// <summary>
		/// Device tokens, or null.
		/// </summary>
		public string[] DeviceTokens
		{
			get { return this.deviceTokens; }
		}

		/// <summary>
		/// Service tokens, or null.
		/// </summary>
		public string[] ServiceTokens
		{
			get { return this.serviceTokens; }
		}

		/// <summary>
		/// User tokens, or null.
		/// </summary>
		public string[] UserTokens
		{
			get { return this.userTokens; }
		}
	}
}
