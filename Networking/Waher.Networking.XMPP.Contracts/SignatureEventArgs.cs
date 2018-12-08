using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for signature callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void SignatureEventHandler(object Sender, SignatureEventArgs e);

	/// <summary>
	/// Event arguments for signature responses
	/// </summary>
	public class SignatureEventArgs : IqResultEventArgs
	{
		private readonly byte[] s1;
		private readonly byte[] s2;

		/// <summary>
		/// Event arguments for signature responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="S1">First signature</param>
		/// <param name="S2">Second signature, if available</param>
		public SignatureEventArgs(IqResultEventArgs e, byte[] S1, byte[] S2)
			: base(e)
		{
			this.s1 = S1;
			this.s2 = S2;
		}

		/// <summary>
		/// First signature
		/// </summary>
		public byte[] S1 => this.s1;

		/// <summary>
		/// Second signature, if available
		/// </summary>
		public byte[] S2 => this.s2;
	}
}
