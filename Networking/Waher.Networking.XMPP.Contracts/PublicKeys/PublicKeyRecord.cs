using System;
using Waher.Networking.XMPP.Contracts.EventArguments;

namespace Waher.Networking.XMPP.Contracts.PublicKeys
{
	/// <summary>
	/// Contains information about a public key.
	/// </summary>
	public class PublicKeyRecord
	{
		/// <summary>
		/// Contains information about a public key.
		/// </summary>
		/// <param name="From">From when the key is valid.</param>
		///	<param name="To">To when the key is valid.</param>
		///	<param name="PublicKey">Public Key.</param>
		public PublicKeyRecord(DateTime From, DateTime To, KeyEventArgs PublicKey)
		{
			this.From = From;
			this.To = To;
			this.PublicKey = PublicKey;
		}

		/// <summary>
		/// From when the key is valid.
		/// </summary>
		public DateTime From { get; internal set; }

		/// <summary>
		/// To when the key is valid.
		/// </summary>
		public DateTime To { get; internal set; }

		/// <summary>
		/// Public Key.
		/// </summary>
		public KeyEventArgs PublicKey { get; }
	}
}
