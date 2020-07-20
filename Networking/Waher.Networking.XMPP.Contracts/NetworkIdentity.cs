using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains a network identity related to a legal identity
	/// </summary>
	public class NetworkIdentity
	{
		private readonly string bareJid;
		private readonly string legalId;

        /// <summary>
        /// Contains a network identity related to a legal identity
        /// </summary>
        public NetworkIdentity()
        {
            this.bareJid = string.Empty;
            this.legalId = string.Empty;
        }

        /// <summary>
        /// Contains a network identity related to a legal identity
        /// </summary>
        /// <param name="BareJid">Bare JID</param>
        /// <param name="LegalId">Legal ID</param>
        public NetworkIdentity(string BareJid, string LegalId)
		{
			this.bareJid = BareJid;
			this.legalId = LegalId;
		}

		/// <summary>
		/// Bare JID
		/// </summary>
		public string BareJid => this.bareJid;

		/// <summary>
		/// Legal ID
		/// </summary>
		public string LegalId => this.legalId;

		/// <summary>
		/// Legal ID, as an URI.
		/// </summary>
		public Uri LegalIdUri => ContractsClient.LegalIdUri(this.legalId);

		/// <summary>
		/// Legal ID, as an URI string.
		/// </summary>
		public string LegalIdUriString => ContractsClient.LegalIdUriString(this.legalId);
	}
}
