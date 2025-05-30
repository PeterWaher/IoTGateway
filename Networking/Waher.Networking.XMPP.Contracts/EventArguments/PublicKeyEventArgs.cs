using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for public key request events.
	/// </summary>
	public class PublicKeyEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for public key request events.
		/// </summary>
		/// <param name="Address">Address whose public key is requested</param>
		public PublicKeyEventArgs(string Address)
			: base()
		{
			this.Address = Address;
			this.Key = null;
		}

		/// <summary>
		/// Address whose public key is requested
		/// </summary>
		public string Address { get; }

		/// <summary>
		/// Public key of endpoint corresponding to <see cref="Address"/>.
		/// </summary>
		public IE2eEndpoint Key { get; set; }
	}
}
