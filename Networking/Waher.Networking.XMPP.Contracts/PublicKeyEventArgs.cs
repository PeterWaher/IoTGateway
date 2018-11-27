using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for public key callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void PublicKeyEventHandler(object Sender, PublicKeyEventArgs e);

	/// <summary>
	/// Event arguments for public key responses
	/// </summary>
	public class PublicKeyEventArgs : IqResultEventArgs
	{
		private readonly IE2eEndpoint serverEndpoint;

		/// <summary>
		/// Event arguments for public key responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="ServerEndpoint">Public key of server endpoint.</param>
		public PublicKeyEventArgs(IqResultEventArgs e, IE2eEndpoint ServerEndpoint)
			: base(e)
		{
			this.serverEndpoint = ServerEndpoint;
		}

		/// <summary>
		/// Public key of server endpoint.
		/// </summary>
		public IE2eEndpoint ServerEndpoint => this.serverEndpoint;
	}
}
